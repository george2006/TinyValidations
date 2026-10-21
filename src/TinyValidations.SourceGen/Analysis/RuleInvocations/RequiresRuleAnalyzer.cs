using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class RequiresRuleAnalyzer
    {
        private readonly MemberAccessAnalyzer _memberAccessAnalyzer = new MemberAccessAnalyzer();
        private readonly RuleArgumentAnalyzer _argumentAnalyzer = new RuleArgumentAnalyzer();

        public RuleAnalysisResult Analyze(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocation)
        {
            if (!HasSupportedArgumentCount(invocation))
            {
                return RuleAnalysisIssue.UnsupportedArgument(invocation, invocation.ToString());
            }

            var selectorArgument = invocation.ArgumentList.Arguments[0];
            var requirementArgument = invocation.ArgumentList.Arguments[1];
            var messageArgument = invocation.ArgumentList.Arguments[2];

            var member = AnalyzeSelector(selectorArgument);
            if (member == null)
            {
                return RuleAnalysisIssue.UnsupportedSelector(
                    selectorArgument,
                    selectorArgument.Expression.ToString());
            }

            if (!IsSupportedRequirementMethod(semanticModel, requirementArgument.Expression, out var requirementMethod))
            {
                return RuleAnalysisIssue.UnsupportedArgument(
                    requirementArgument,
                    requirementArgument.Expression.ToString());
            }

            if (!IsSupportedMessage(semanticModel, messageArgument))
            {
                return RuleAnalysisIssue.UnsupportedArgument(
                    messageArgument,
                    messageArgument.Expression.ToString());
            }

            var message = _argumentAnalyzer.GetMessage(semanticModel, messageArgument.Expression);

            return CreateRule(member, requirementMethod, message);
        }

        private RuleAnalysisResult CreateRule(
            AnalyzedMemberAccess member,
            string requirementMethod,
            string message)
        {
            return RuleAnalysisResult.ForRule(new RuleDefinition(
                kind: RuleKind.Requires,
                memberPath: member.Path,
                memberAccess: member.Access,
                argument: string.Empty,
                argumentDisplay: string.Empty,
                message: message,
                customRuleType: string.Empty,
                requirementMethod: requirementMethod));
        }

        private AnalyzedMemberAccess? AnalyzeSelector(ArgumentSyntax selectorArgument)
        {
            return _memberAccessAnalyzer.Analyze(selectorArgument.Expression);
        }

        private static bool HasSupportedArgumentCount(InvocationExpressionSyntax invocation)
        {
            return invocation.ArgumentList.Arguments.Count == RuleShape.MinimumArgumentCount(RuleKind.Requires);
        }

        private static bool IsSupportedMessage(SemanticModel semanticModel, ArgumentSyntax messageArgument)
        {
            var constant = semanticModel.GetConstantValue(messageArgument.Expression);
            return constant.HasValue;
        }

        private static bool IsSupportedRequirementMethod(
            SemanticModel semanticModel,
            ExpressionSyntax expression,
            out string requirementMethod)
        {
            requirementMethod = string.Empty;

            var symbolInfo = semanticModel.GetSymbolInfo(expression);
            var symbol = symbolInfo.Symbol ?? GetSingleCandidate(symbolInfo);
            if (!(symbol is IMethodSymbol method))
            {
                return false;
            }

            if (!method.IsStatic)
            {
                return false;
            }

            if (method.TypeArguments.Length != 0)
            {
                return false;
            }

            if (method.Parameters.Length != 1)
            {
                return false;
            }

            if (method.ReturnType.SpecialType != SpecialType.System_Boolean)
            {
                return false;
            }

            requirementMethod = GetRequirementMethodName(method);
            return true;
        }

        private static ISymbol? GetSingleCandidate(SymbolInfo symbolInfo)
        {
            if (symbolInfo.CandidateSymbols.Length != 1)
            {
                return null;
            }

            return symbolInfo.CandidateSymbols[0];
        }

        private static string GetRequirementMethodName(IMethodSymbol method)
        {
            var containingType = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return containingType + "." + method.Name;
        }
    }
}
