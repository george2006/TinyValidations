using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class MemberRuleAnalyzer
    {
        private readonly MemberAccessAnalyzer _memberAccessAnalyzer = new MemberAccessAnalyzer();
        private readonly RuleArgumentAnalyzer _argumentAnalyzer = new RuleArgumentAnalyzer();

        public RuleAnalysisResult Analyze(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation)
        {
            if (!HasSelector(invocation))
            {
                return RuleAnalysisIssue.UnsupportedSelector(invocation, invocation.ToString());
            }

            var selectorArgument = invocation.ArgumentList.Arguments[0];
            var member = AnalyzeSelector(selectorArgument);
            if (member == null)
            {
                return RuleAnalysisIssue.UnsupportedSelector(
                    selectorArgument,
                    selectorArgument.Expression.ToString());
            }

            if (HasUnsupportedArgument(kind, invocation))
            {
                return RuleAnalysisIssue.UnsupportedArgument(invocation, invocation.ToString());
            }

            return CreateRule(semanticModel, kind, invocation, member);
        }

        private RuleAnalysisResult CreateRule(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation,
            AnalyzedMemberAccess member)
        {
            var argument = _argumentAnalyzer.GetRuleArgument(kind, invocation);
            var argumentDisplay = _argumentAnalyzer.GetRuleArgumentDisplay(kind, invocation);
            var message = _argumentAnalyzer.GetMessage(kind, invocation);
            var comparisonTypeName = GetComparisonTypeName(semanticModel, kind, invocation);

            return RuleAnalysisResult.ForRule(new RuleDefinition(
                kind,
                member.Path,
                member.Access,
                argument,
                argumentDisplay,
                message,
                string.Empty,
                comparisonTypeName: comparisonTypeName));
        }

        private AnalyzedMemberAccess? AnalyzeSelector(ArgumentSyntax selectorArgument)
        {
            return _memberAccessAnalyzer.Analyze(selectorArgument.Expression);
        }

        private static bool HasSelector(InvocationExpressionSyntax invocation)
        {
            return invocation.ArgumentList.Arguments.Count > 0;
        }

        private static bool HasUnsupportedArgument(RuleKind kind, InvocationExpressionSyntax invocation)
        {
            if (HasUnsupportedValueArgument(kind, invocation))
            {
                return true;
            }

            return HasUnsupportedMessageArgument(kind, invocation);
        }

        private static bool HasUnsupportedValueArgument(RuleKind kind, InvocationExpressionSyntax invocation)
        {
            var valueArgumentIndex = RuleShape.ValueArgumentIndex(kind);
            if (valueArgumentIndex < 0)
            {
                return false;
            }

            return !IsSupportedArgument(invocation, valueArgumentIndex);
        }

        private static bool HasUnsupportedMessageArgument(RuleKind kind, InvocationExpressionSyntax invocation)
        {
            var messageArgumentIndex = RuleShape.MessageArgumentIndex(kind);
            if (!HasArgument(invocation, messageArgumentIndex))
            {
                return false;
            }

            return !IsSupportedArgument(invocation, messageArgumentIndex);
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

        private static string GetComparisonTypeName(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation)
        {
            if (!IsComparableRule(kind))
            {
                return string.Empty;
            }

            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;
            if (!(symbol is IMethodSymbol method))
            {
                return string.Empty;
            }

            if (method.TypeArguments.Length != 1)
            {
                return string.Empty;
            }

            return method.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        private static bool IsComparableRule(RuleKind kind)
        {
            if (kind == RuleKind.Above)
            {
                return true;
            }

            if (kind == RuleKind.AtLeast)
            {
                return true;
            }

            if (kind == RuleKind.Below)
            {
                return true;
            }

            return kind == RuleKind.AtMost;
        }
    }
}
