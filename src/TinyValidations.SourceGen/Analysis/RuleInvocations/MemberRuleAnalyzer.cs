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
        private readonly RuleArgumentAnalyzer _argumentAnalyzer = new RuleArgumentAnalyzer();

        public RuleAnalysisResult Analyze(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation)
        {
            if (!HasSupportedArgumentCount(kind, invocation))
            {
                return RuleAnalysisIssue.UnsupportedArgument(invocation, invocation.ToString());
            }

            var selectorArgument = _argumentAnalyzer.FindArgument(semanticModel, invocation, 0);
            if (selectorArgument == null)
            {
                return RuleAnalysisIssue.UnsupportedArgument(invocation, invocation.ToString());
            }

            var member = AnalyzeSelector(selectorArgument);
            if (member == null)
            {
                return RuleAnalysisIssue.UnsupportedSelector(
                    selectorArgument,
                    selectorArgument.Expression.ToString());
            }

            if (HasUnsupportedArgument(semanticModel, kind, invocation))
            {
                return RuleAnalysisIssue.UnsupportedArgument(invocation, invocation.ToString());
            }

            if (HasInvalidRegexPattern(semanticModel, kind, invocation))
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
            var argument = _argumentAnalyzer.GetRuleArgument(semanticModel, kind, invocation);
            var argumentDisplay = _argumentAnalyzer.GetRuleArgumentDisplay(semanticModel, kind, invocation);
            var message = GetMessage(semanticModel, kind, invocation);
            var comparisonTypeName = GetComparisonTypeName(semanticModel, kind, invocation, member);

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

        private string GetMessage(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation)
        {
            var messageArgumentIndex = RuleShape.MessageArgumentIndex(kind);
            var argument = _argumentAnalyzer.FindArgument(semanticModel, invocation, messageArgumentIndex);
            if (argument == null)
            {
                return string.Empty;
            }

            return _argumentAnalyzer.GetMessage(semanticModel, argument.Expression);
        }

        private AnalyzedMemberAccess? AnalyzeSelector(ArgumentSyntax selectorArgument)
        {
            return _memberAccessAnalyzer.Analyze(selectorArgument.Expression);
        }

        private static bool HasSupportedArgumentCount(RuleKind kind, InvocationExpressionSyntax invocation)
        {
            var count = invocation.ArgumentList.Arguments.Count;
            return count >= RuleShape.MinimumArgumentCount(kind)
                && count <= RuleShape.MaximumArgumentCount(kind);
        }

        private bool HasUnsupportedArgument(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation)
        {
            if (HasUnsupportedValueArgument(semanticModel, kind, invocation))
            {
                return true;
            }

            return HasUnsupportedMessageArgument(semanticModel, kind, invocation);
        }

        private bool HasUnsupportedValueArgument(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation)
        {
            var valueArgumentIndex = RuleShape.ValueArgumentIndex(kind);
            if (valueArgumentIndex < 0)
            {
                return false;
            }

            var argument = _argumentAnalyzer.FindArgument(semanticModel, invocation, valueArgumentIndex);
            if (argument == null)
            {
                return true;
            }

            var constant = semanticModel.GetConstantValue(argument.Expression);

            return !constant.HasValue;
        }

        private bool HasUnsupportedMessageArgument(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation)
        {
            var messageArgumentIndex = RuleShape.MessageArgumentIndex(kind);
            var argument = _argumentAnalyzer.FindArgument(semanticModel, invocation, messageArgumentIndex);
            if (argument == null)
            {
                return false;
            }

            var constant = semanticModel.GetConstantValue(argument.Expression);

            return !constant.HasValue;
        }

        private bool HasInvalidRegexPattern(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation)
        {
            if (kind != RuleKind.Matches)
            {
                return false;
            }

            var argumentIndex = RuleShape.ValueArgumentIndex(kind);
            var argument = _argumentAnalyzer.FindArgument(semanticModel, invocation, argumentIndex);
            if (argument == null)
            {
                return true;
            }

            var constant = semanticModel.GetConstantValue(argument.Expression);
            if (!(constant.Value is string pattern))
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
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation,
            AnalyzedMemberAccess member)
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

            var typeArgument = method.TypeArguments[0];
            if (RequiresNullableComparisonType(typeArgument, member))
            {
                return "global::System.Nullable<" + typeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ">";
            }

            return typeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
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
    }
}
