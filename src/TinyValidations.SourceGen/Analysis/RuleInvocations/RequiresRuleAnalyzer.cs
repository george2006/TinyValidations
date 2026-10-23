using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class RequiresRuleAnalyzer
    {
        private readonly MemberAccessAnalyzer _memberAccessAnalyzer = new MemberAccessAnalyzer();
        private readonly ConstantArgumentFormatter _argumentFormatter = new ConstantArgumentFormatter();

        public RuleAnalysisResult Analyze(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocation)
        {
            var call = new RequiresRuleInvocation(semanticModel, invocation);

            var argumentIssue = ValidateArgumentList(call);
            if (argumentIssue != null)
            {
                return argumentIssue;
            }

            var member = AnalyzeMember(call);
            if (member == null)
            {
                return UnsupportedSelector(call);
            }

            var requirementIssue = ValidateRequirement(call);
            if (requirementIssue != null)
            {
                return requirementIssue;
            }

            var messageIssue = ValidateMessage(call);
            if (messageIssue != null)
            {
                return messageIssue;
            }

            return CreateRule(call, member);
        }

        private static RuleAnalysisResult? ValidateArgumentList(RequiresRuleInvocation call)
        {
            if (!HasSupportedArgumentCount(call))
            {
                return UnsupportedArgument(call);
            }

            if (call.SelectorArgument == null)
            {
                return UnsupportedArgument(call);
            }

            return null;
        }

        private AnalyzedMemberAccess? AnalyzeMember(RequiresRuleInvocation call)
        {
            var selector = call.SelectorArgument!.Expression;
            return _memberAccessAnalyzer.Analyze(selector);
        }

        private static RuleAnalysisResult? ValidateRequirement(RequiresRuleInvocation call)
        {
            if (call.RequirementArgument == null)
            {
                return UnsupportedArgument(call);
            }

            if (!IsSupportedRequirementMethod(call.RequirementMethod))
            {
                return UnsupportedRequirement(call);
            }

            return null;
        }

        private static RuleAnalysisResult? ValidateMessage(RequiresRuleInvocation call)
        {
            if (call.MessageArgument == null)
            {
                return UnsupportedArgument(call);
            }

            if (!call.MessageConstant.HasValue)
            {
                return UnsupportedMessage(call);
            }

            return null;
        }

        private RuleAnalysisResult CreateRule(
            RequiresRuleInvocation call,
            AnalyzedMemberAccess member)
        {
            var requirementMethod = GetRequirementMethodName(call.RequirementMethod!);
            var message = _argumentFormatter.FormatExpression(call.MessageConstant.Value);

            var rule = new RuleDefinition(
                kind: RuleKind.Requires,
                memberPath: member.Path,
                memberAccess: member.Access,
                argument: string.Empty,
                argumentDisplay: string.Empty,
                message: message,
                customRuleType: string.Empty,
                requirementMethod: requirementMethod);

            return RuleAnalysisResult.ForRule(rule);
        }

        private static bool HasSupportedArgumentCount(RequiresRuleInvocation call)
        {
            return call.Syntax.ArgumentList.Arguments.Count == RuleShape.MinimumArgumentCount(RuleKind.Requires);
        }

        private static bool IsSupportedRequirementMethod(IMethodSymbol? method)
        {
            if (method == null)
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

            return method.ReturnType.SpecialType == SpecialType.System_Boolean;
        }

        private static string GetRequirementMethodName(IMethodSymbol method)
        {
            var containingType = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return containingType + "." + method.Name;
        }

        private static RuleAnalysisResult UnsupportedArgument(RequiresRuleInvocation call)
        {
            var argument = call.Syntax.ToString();
            return RuleAnalysisIssue.UnsupportedArgument(call.Syntax, argument);
        }

        private static RuleAnalysisResult UnsupportedSelector(RequiresRuleInvocation call)
        {
            var selector = call.SelectorArgument!;
            var expression = selector.Expression.ToString();
            return RuleAnalysisIssue.UnsupportedSelector(selector, expression);
        }

        private static RuleAnalysisResult UnsupportedRequirement(RequiresRuleInvocation call)
        {
            var requirement = call.RequirementArgument!;
            var expression = requirement.Expression.ToString();
            return RuleAnalysisIssue.UnsupportedArgument(requirement, expression);
        }

        private static RuleAnalysisResult UnsupportedMessage(RequiresRuleInvocation call)
        {
            var message = call.MessageArgument!;
            var expression = message.Expression.ToString();
            return RuleAnalysisIssue.UnsupportedArgument(message, expression);
        }
    }
}
