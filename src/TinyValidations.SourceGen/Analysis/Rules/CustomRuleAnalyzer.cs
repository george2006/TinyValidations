using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;

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
                return RuleAnalysisIssue.InvalidCustomRule(methodName, methodName.ToString());
            }

            if (!HasSingleTypeArgument(genericName))
            {
                return RuleAnalysisIssue.InvalidCustomRule(genericName, genericName.ToString());
            }

            var typeSyntax = genericName.TypeArgumentList.Arguments[0];
            var typeSymbol = semanticModel.GetTypeInfo(typeSyntax).Type;
            if (!IsValidCustomRule(typeSymbol, commandType))
            {
                return RuleAnalysisIssue.InvalidCustomRule(typeSyntax, typeSyntax.ToString());
            }

            var customRuleType = GetTypeName(typeSyntax, typeSymbol);
            return CreateRule(customRuleType);
        }

        private static RuleAnalysisResult CreateRule(string customRuleType)
        {
            return RuleAnalysisResult.ForRule(new RuleDefinition(
                RuleKind.Use,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                customRuleType));
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
            if (!IsTinyValidationsRule(candidate))
            {
                return false;
            }

            if (!HasSingleTypeArgument(candidate))
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(candidate.TypeArguments[0], commandType);
        }

        private static bool IsTinyValidationsRule(INamedTypeSymbol candidate)
        {
            if (candidate.ContainingNamespace.ToDisplayString() != "TinyValidations")
            {
                return false;
            }

            return candidate.Name == "IAsyncValidationRule";
        }

        private static bool HasSingleTypeArgument(INamedTypeSymbol candidate)
        {
            return candidate.TypeArguments.Length == 1;
        }
    }
}
