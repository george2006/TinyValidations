using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.Rules
{
    internal sealed class RuleInvocationAnalyzer
    {
        private readonly RuleMethodMap _methodMap = new RuleMethodMap();
        private readonly MemberRuleAnalyzer _memberRuleAnalyzer = new MemberRuleAnalyzer();
        private readonly CustomRuleAnalyzer _customRuleAnalyzer = new CustomRuleAnalyzer();
        private readonly RequiresRuleAnalyzer _requiresRuleAnalyzer = new RequiresRuleAnalyzer();

        public RuleAnalysisResult? Analyze(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocation,
            INamedTypeSymbol validationRules,
            INamedTypeSymbol commandType)
        {
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
            {
                return null;
            }

            if (!IsValidationRulesInvocation(semanticModel, memberAccess, invocation, validationRules))
            {
                return null;
            }

            var methodName = memberAccess.Name.Identifier.ValueText;
            var kind = _methodMap.GetKind(methodName);
            if (kind == null)
            {
                return RuleAnalysisIssue.UnsupportedRuleCall(memberAccess.Name, methodName);
            }

            if (kind.Value == RuleKind.Use)
            {
                return _customRuleAnalyzer.Analyze(semanticModel, memberAccess.Name, commandType);
            }

            if (kind.Value == RuleKind.Requires)
            {
                return _requiresRuleAnalyzer.Analyze(semanticModel, invocation);
            }

            return _memberRuleAnalyzer.Analyze(kind.Value, invocation);
        }

        private static bool IsValidationRulesInvocation(
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
