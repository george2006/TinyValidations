using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Analysis.RuleInvocations;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.Declarations
{
    internal sealed class DefineMethodAnalyzer
    {
        private readonly RuleInvocationAnalyzer _ruleInvocationAnalyzer = new RuleInvocationAnalyzer();

        public ValidationDefinition? Analyze(
            SemanticModel semanticModel,
            ClassDeclarationSyntax declaration,
            INamedTypeSymbol validationType,
            INamedTypeSymbol commandType,
            INamedTypeSymbol validationRules,
            List<ValidationIssue> issues)
        {
            var defineMethod = declaration.Members
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(method => IsDefineMethod(semanticModel, method, commandType, validationRules));

            if (defineMethod == null)
            {
                return null;
            }

            var invocations = defineMethod.DescendantNodes().OfType<InvocationExpressionSyntax>();
            var rules = new List<RuleDefinition>();

            foreach (var invocation in invocations)
            {
                var result = _ruleInvocationAnalyzer.Analyze(semanticModel, invocation, validationRules, commandType);
                if (result == null)
                {
                    continue;
                }

                if (result.Issue != null)
                {
                    issues.Add(result.Issue);
                    continue;
                }

                if (result.Rule != null)
                {
                    rules.Add(result.Rule);
                }
            }

            return new ValidationDefinition(
                declaration.Identifier.GetLocation(),
                validationType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                commandType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                rules);
        }

        private static bool IsDefineMethod(
            SemanticModel semanticModel,
            MethodDeclarationSyntax method,
            INamedTypeSymbol commandType,
            INamedTypeSymbol validationRules)
        {
            if (method.Identifier.ValueText != "Define")
            {
                return false;
            }

            if (!HasSingleParameter(method))
            {
                return false;
            }

            if (!ReturnsVoid(semanticModel, method))
            {
                return false;
            }

            return HasValidationRulesParameter(semanticModel, method, commandType, validationRules);
        }

        private static bool HasSingleParameter(MethodDeclarationSyntax method)
        {
            return method.ParameterList.Parameters.Count == 1;
        }

        private static bool ReturnsVoid(SemanticModel semanticModel, MethodDeclarationSyntax method)
        {
            var symbol = semanticModel.GetDeclaredSymbol(method);
            if (!(symbol is IMethodSymbol methodSymbol))
            {
                return false;
            }

            return methodSymbol.ReturnsVoid;
        }

        private static bool HasValidationRulesParameter(
            SemanticModel semanticModel,
            MethodDeclarationSyntax method,
            INamedTypeSymbol commandType,
            INamedTypeSymbol validationRules)
        {
            var parameter = method.ParameterList.Parameters[0];
            var type = semanticModel.GetTypeInfo(parameter.Type!).Type;

            if (!(type is INamedTypeSymbol namedType))
            {
                return false;
            }

            if (!SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, validationRules))
            {
                return false;
            }

            if (namedType.TypeArguments.Length != 1)
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], commandType);
        }
    }
}
