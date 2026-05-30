using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Validation;

namespace TinyValidations.SourceGen.Analysis.Rules
{
    internal sealed class MemberRuleAnalyzer
    {
        private readonly MemberAccessAnalyzer _memberAccessAnalyzer = new MemberAccessAnalyzer();
        private readonly RuleArgumentAnalyzer _argumentAnalyzer = new RuleArgumentAnalyzer();

        public RuleAnalysisResult Analyze(RuleKind kind, InvocationExpressionSyntax invocation)
        {
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

            if (HasUnsupportedArgument(kind, invocation))
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedArgument,
                    invocation.GetLocation(),
                    invocation.ToString()));
            }

            var argument = _argumentAnalyzer.GetRuleArgument(kind, invocation);
            var message = _argumentAnalyzer.GetMessage(kind, invocation);

            return RuleAnalysisResult.ForRule(new RuleDefinition(
                kind,
                member.Path,
                member.Access,
                argument,
                message,
                string.Empty));
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

        private static bool IsSupportedArgument(InvocationExpressionSyntax invocation, int argumentIndex)
        {
            if (!HasArgument(invocation, argumentIndex))
            {
                return false;
            }

            return invocation.ArgumentList.Arguments[argumentIndex].Expression is LiteralExpressionSyntax;
        }

        private static bool HasArgument(InvocationExpressionSyntax invocation, int argumentIndex)
        {
            return invocation.ArgumentList.Arguments.Count > argumentIndex;
        }
    }
}
