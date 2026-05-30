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
            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;
            if (symbol is IMethodSymbol method)
            {
                return SymbolEqualityComparer.Default.Equals(method.ContainingType.OriginalDefinition, validationRules);
            }

            var expressionType = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
            if (!(expressionType is INamedTypeSymbol namedType))
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, validationRules);
        }
    }
}
