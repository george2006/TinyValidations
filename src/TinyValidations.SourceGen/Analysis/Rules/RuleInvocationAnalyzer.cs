using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Validation;

namespace TinyValidations.SourceGen.Analysis.Rules
{
    internal sealed class RuleInvocationAnalyzer
    {
        private readonly RuleMethodMap _methodMap = new RuleMethodMap();
        private readonly MemberAccessAnalyzer _memberAccessAnalyzer = new MemberAccessAnalyzer();
        private readonly RuleArgumentAnalyzer _argumentAnalyzer = new RuleArgumentAnalyzer();
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
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedRuleCall,
                    memberAccess.Name.GetLocation(),
                    methodName));
            }

            if (kind.Value == RuleKind.Use)
            {
                return _customRuleAnalyzer.Analyze(semanticModel, memberAccess.Name, commandType);
            }

            if (kind.Value == RuleKind.Requires)
            {
                return _requiresRuleAnalyzer.Analyze(semanticModel, invocation);
            }

            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedSelector,
                    invocation.GetLocation(),
                    invocation.ToString()));
            }

            var member = _memberAccessAnalyzer.Analyze(invocation.ArgumentList.Arguments[0].Expression);
            if (member == null)
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedSelector,
                    invocation.ArgumentList.Arguments[0].GetLocation(),
                    invocation.ArgumentList.Arguments[0].Expression.ToString()));
            }

            if (HasUnsupportedArgument(kind.Value, invocation))
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedArgument,
                    invocation.GetLocation(),
                    invocation.ToString()));
            }

            var argument = _argumentAnalyzer.GetRuleArgument(kind.Value, invocation);
            var message = _argumentAnalyzer.GetMessage(kind.Value, invocation);

            return RuleAnalysisResult.ForRule(new RuleDefinition(kind.Value, member.Path, member.Access, argument, message, string.Empty));
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

        private static bool HasUnsupportedArgument(RuleKind kind, InvocationExpressionSyntax invocation)
        {
            var valueArgumentIndex = RuleShape.ValueArgumentIndex(kind);
            if (valueArgumentIndex >= 0)
            {
                if (!IsSupportedArgument(invocation, valueArgumentIndex))
                {
                    return true;
                }
            }

            var messageArgumentIndex = RuleShape.MessageArgumentIndex(kind);
            if (HasArgument(invocation, messageArgumentIndex))
            {
                if (!IsSupportedArgument(invocation, messageArgumentIndex))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasArgument(InvocationExpressionSyntax invocation, int argumentIndex)
        {
            return invocation.ArgumentList.Arguments.Count > argumentIndex;
        }

        private static bool IsSupportedArgument(InvocationExpressionSyntax invocation, int argumentIndex)
        {
            if (!HasArgument(invocation, argumentIndex))
            {
                return false;
            }

            return invocation.ArgumentList.Arguments[argumentIndex].Expression is LiteralExpressionSyntax;
        }

    }
}
