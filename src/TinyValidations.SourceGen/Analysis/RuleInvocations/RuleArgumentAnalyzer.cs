using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class RuleArgumentAnalyzer
    {
        public string GetRuleArgument(RuleKind kind, InvocationExpressionSyntax invocation)
        {
            if (!RuleShape.RequiresValueArgument(kind))
            {
                return string.Empty;
            }

            return GetArgument(invocation, RuleShape.ValueArgumentIndex(kind));
        }

        public string GetMessage(RuleKind kind, InvocationExpressionSyntax invocation)
        {
            var messageIndex = RuleShape.MessageArgumentIndex(kind);
            return GetArgument(invocation, messageIndex);
        }

        private static string GetArgument(InvocationExpressionSyntax invocation, int argumentIndex)
        {
            if (!HasArgument(invocation, argumentIndex))
            {
                return string.Empty;
            }

            return invocation.ArgumentList.Arguments[argumentIndex].Expression.ToString();
        }

        private static bool HasArgument(InvocationExpressionSyntax invocation, int argumentIndex)
        {
            return invocation.ArgumentList.Arguments.Count > argumentIndex;
        }
    }
}
