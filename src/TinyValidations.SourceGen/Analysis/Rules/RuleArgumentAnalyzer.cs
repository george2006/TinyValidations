using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.Rules
{
    internal sealed class RuleArgumentAnalyzer
    {
        public string GetRuleArgument(RuleKind kind, InvocationExpressionSyntax invocation)
        {
            if (!RequiresValueArgument(kind))
            {
                return string.Empty;
            }

            return GetArgument(invocation, 1);
        }

        public string GetMessage(RuleKind kind, InvocationExpressionSyntax invocation)
        {
            var messageIndex = GetMessageArgumentIndex(kind);
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

        private static int GetMessageArgumentIndex(RuleKind kind)
        {
            if (RequiresValueArgument(kind))
            {
                return 2;
            }

            return 1;
        }

        private static bool HasArgument(InvocationExpressionSyntax invocation, int argumentIndex)
        {
            return invocation.ArgumentList.Arguments.Count > argumentIndex;
        }

        private static bool RequiresValueArgument(RuleKind kind)
        {
            return kind == RuleKind.TextLengthAtLeast
                || kind == RuleKind.TextLengthAtMost
                || kind == RuleKind.Above
                || kind == RuleKind.AtLeast
                || kind == RuleKind.Below
                || kind == RuleKind.AtMost
                || kind == RuleKind.Matches;
        }
    }
}
