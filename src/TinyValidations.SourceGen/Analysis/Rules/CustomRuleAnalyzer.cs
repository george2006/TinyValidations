using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Validation;

namespace TinyValidations.SourceGen.Analysis.Rules
{
    internal sealed class CustomRuleAnalyzer
    {
        public RuleAnalysisResult Analyze(
            SemanticModel semanticModel,
            SimpleNameSyntax methodName,
            INamedTypeSymbol commandType)
        {
            if (!(methodName is GenericNameSyntax genericName))
            {
                return InvalidCustomRule(methodName, methodName.ToString());
            }

            if (!HasSingleTypeArgument(genericName))
            {
                return InvalidCustomRule(genericName, genericName.ToString());
            }

            var typeSyntax = genericName.TypeArgumentList.Arguments[0];
            var typeSymbol = semanticModel.GetTypeInfo(typeSyntax).Type;
            if (!IsValidCustomRule(typeSymbol, commandType))
            {
                return InvalidCustomRule(typeSyntax, typeSyntax.ToString());
            }

            var customRuleType = GetTypeName(typeSyntax, typeSymbol);

            return RuleAnalysisResult.ForRule(new RuleDefinition(
                RuleKind.Use,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                customRuleType));
        }

        private static RuleAnalysisResult InvalidCustomRule(SyntaxNode syntax, string value)
        {
            return RuleAnalysisResult.ForIssue(new ValidationIssue(
                ValidationDiagnostics.InvalidCustomRule,
                syntax.GetLocation(),
                value));
        }

        private static bool HasSingleTypeArgument(GenericNameSyntax genericName)
        {
            return genericName.TypeArgumentList.Arguments.Count == 1;
        }

        private static string GetTypeName(TypeSyntax typeSyntax, ITypeSymbol? typeSymbol)
        {
            if (typeSymbol == null)
            {
                return typeSyntax.ToString();
            }

            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        private static bool IsValidCustomRule(ITypeSymbol? typeSymbol, INamedTypeSymbol commandType)
        {
            if (!(typeSymbol is INamedTypeSymbol namedType))
            {
                return false;
            }

            foreach (var candidate in namedType.AllInterfaces)
            {
                if (IsAsyncValidationRule(candidate, commandType))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAsyncValidationRule(INamedTypeSymbol candidate, INamedTypeSymbol commandType)
        {
            if (candidate.ContainingNamespace.ToDisplayString() != "TinyValidations")
            {
                return false;
            }

            if (candidate.Name != "IAsyncValidationRule")
            {
                return false;
            }

            if (candidate.TypeArguments.Length != 1)
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(candidate.TypeArguments[0], commandType);
        }
    }
}
