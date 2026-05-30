using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TinyValidations.SourceGen.Analysis.Rules
{
    internal sealed class ValidationRulesInvocationMatcher
    {
        public bool IsMatch(
            SemanticModel semanticModel,
            MemberAccessExpressionSyntax memberAccess,
            InvocationExpressionSyntax invocation,
            INamedTypeSymbol validationRules)
        {
            if (MatchesResolvedMethod(semanticModel, invocation, validationRules))
            {
                return true;
            }

            return MatchesMemberAccessExpression(semanticModel, memberAccess, validationRules);
        }

        private static bool MatchesResolvedMethod(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocation,
            INamedTypeSymbol validationRules)
        {
            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;
            if (!(symbol is IMethodSymbol method))
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(method.ContainingType.OriginalDefinition, validationRules);
        }

        private static bool MatchesMemberAccessExpression(
            SemanticModel semanticModel,
            MemberAccessExpressionSyntax memberAccess,
            INamedTypeSymbol validationRules)
        {
            var expressionType = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
            if (!(expressionType is INamedTypeSymbol namedType))
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, validationRules);
        }
    }
}
