using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Analysis.Rules;
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
                .FirstOrDefault(IsDefineMethod);

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

        private static bool IsDefineMethod(MethodDeclarationSyntax method)
        {
            if (method.Identifier.ValueText != "Define")
            {
                return false;
            }

            return HasSingleParameter(method);
        }

        private static bool HasSingleParameter(MethodDeclarationSyntax method)
        {
            return method.ParameterList.Parameters.Count == 1;
        }
    }
}
