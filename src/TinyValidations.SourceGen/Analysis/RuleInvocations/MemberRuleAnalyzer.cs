using System;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class MemberRuleAnalyzer
    {
        private readonly MemberAccessAnalyzer _memberAccessAnalyzer = new MemberAccessAnalyzer();
        private readonly ConstantArgumentFormatter _argumentFormatter = new ConstantArgumentFormatter();

        public RuleAnalysisResult Analyze(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation)
        {
            var call = new MemberRuleInvocation(semanticModel, kind, invocation);

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

            var ruleIssue = ValidateRuleArguments(call);
            if (ruleIssue != null)
            {
                return ruleIssue;
            }

            return CreateRule(call, member);
        }

        private static RuleAnalysisResult? ValidateArgumentList(MemberRuleInvocation call)
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

        private AnalyzedMemberAccess? AnalyzeMember(MemberRuleInvocation call)
        {
            var selector = call.SelectorArgument!.Expression;
            return _memberAccessAnalyzer.Analyze(selector);
        }

        private static RuleAnalysisResult? ValidateRuleArguments(MemberRuleInvocation call)
        {
            if (HasUnsupportedValueArgument(call))
            {
                return UnsupportedArgument(call);
            }

            if (HasUnsupportedMessageArgument(call))
            {
                return UnsupportedArgument(call);
            }

            if (HasInvalidRegexPattern(call))
            {
                return UnsupportedArgument(call);
            }

            return null;
        }

        private RuleAnalysisResult CreateRule(
            MemberRuleInvocation call,
            AnalyzedMemberAccess member)
        {
            var valueExpression = string.Empty;
            var displayText = string.Empty;
            if (call.ValueArgument != null)
            {
                var value = call.ValueConstant.Value;
                valueExpression = _argumentFormatter.FormatExpression(value, call.ValueType);
                displayText = _argumentFormatter.FormatDisplayText(value);
            }

            var message = string.Empty;
            if (call.MessageArgument != null)
            {
                message = _argumentFormatter.FormatExpression(call.MessageConstant.Value);
            }

            var comparisonTypeName = GetComparisonTypeName(call, member);

            var rule = new RuleDefinition(
                kind: call.Kind,
                memberPath: member.Path,
                memberAccess: member.Access,
                argument: valueExpression,
                argumentDisplay: displayText,
                message: message,
                customRuleType: string.Empty,
                comparisonTypeName: comparisonTypeName);

            return RuleAnalysisResult.ForRule(rule);
        }

        private static bool HasSupportedArgumentCount(MemberRuleInvocation call)
        {
            var count = call.Syntax.ArgumentList.Arguments.Count;
            return count >= RuleShape.MinimumArgumentCount(call.Kind)
                && count <= RuleShape.MaximumArgumentCount(call.Kind);
        }

        private static bool HasUnsupportedValueArgument(MemberRuleInvocation call)
        {
            if (!RuleShape.RequiresValueArgument(call.Kind))
            {
                return false;
            }

            return !call.ValueConstant.HasValue;
        }

        private static bool HasUnsupportedMessageArgument(MemberRuleInvocation call)
        {
            if (call.MessageArgument == null)
            {
                return false;
            }

            return !call.MessageConstant.HasValue;
        }

        private static bool HasInvalidRegexPattern(MemberRuleInvocation call)
        {
            if (call.Kind != RuleKind.Matches)
            {
                return false;
            }

            if (!(call.ValueConstant.Value is string pattern))
            {
                return true;
            }

            try
            {
                _ = new Regex(pattern);
                return false;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }

        private static string GetComparisonTypeName(
            MemberRuleInvocation call,
            AnalyzedMemberAccess member)
        {
            if (!IsComparableRule(call.Kind))
            {
                return string.Empty;
            }

            var method = call.Method;
            if (method == null)
            {
                return string.Empty;
            }

            if (method.TypeArguments.Length != 1)
            {
                return string.Empty;
            }

            var typeArgument = method.TypeArguments[0];
            var typeName = typeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (RequiresNullableComparisonType(typeArgument, member))
            {
                return "global::System.Nullable<" + typeName + ">";
            }

            return typeName;
        }

        private static bool RequiresNullableComparisonType(
            ITypeSymbol type,
            AnalyzedMemberAccess member)
        {
            if (!member.IsNullSafe)
            {
                return false;
            }

            if (type.IsReferenceType)
            {
                return false;
            }

            if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                return false;
            }

            return type.IsValueType;
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

        private static RuleAnalysisResult UnsupportedArgument(MemberRuleInvocation call)
        {
            var argument = call.Syntax.ToString();
            return RuleAnalysisIssue.UnsupportedArgument(call.Syntax, argument);
        }

        private static RuleAnalysisResult UnsupportedSelector(MemberRuleInvocation call)
        {
            var selector = call.SelectorArgument!;
            var expression = selector.Expression.ToString();
            return RuleAnalysisIssue.UnsupportedSelector(selector, expression);
        }
    }
}
