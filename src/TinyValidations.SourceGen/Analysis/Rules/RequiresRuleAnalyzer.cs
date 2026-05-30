using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Validation;

namespace TinyValidations.SourceGen.Analysis.Rules
{
    internal sealed class RequiresRuleAnalyzer
    {
        private readonly MemberAccessAnalyzer _memberAccessAnalyzer = new MemberAccessAnalyzer();

        public RuleAnalysisResult Analyze(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocation)
        {
            if (invocation.ArgumentList.Arguments.Count < 3)
            {
                return UnsupportedArgument(invocation, invocation.ToString());
            }

            var member = _memberAccessAnalyzer.Analyze(invocation.ArgumentList.Arguments[0].Expression);
            if (member == null)
            {
                return UnsupportedSelector(
                    invocation.ArgumentList.Arguments[0],
                    invocation.ArgumentList.Arguments[0].Expression.ToString());
            }

            if (!IsSupportedRequirementMethod(semanticModel, invocation.ArgumentList.Arguments[1].Expression, out var requirementMethod))
            {
                return UnsupportedArgument(
                    invocation.ArgumentList.Arguments[1],
                    invocation.ArgumentList.Arguments[1].Expression.ToString());
            }

            if (!IsSupportedArgument(invocation, 2))
            {
                return UnsupportedArgument(
                    invocation.ArgumentList.Arguments[2],
                    invocation.ArgumentList.Arguments[2].Expression.ToString());
            }

            var message = invocation.ArgumentList.Arguments[2].Expression.ToString();

            return RuleAnalysisResult.ForRule(new RuleDefinition(
                RuleKind.Requires,
                member.Path,
                member.Access,
                string.Empty,
                message,
                string.Empty,
                requirementMethod));
        }

        private static RuleAnalysisResult UnsupportedSelector(SyntaxNode syntax, string value)
        {
            return RuleAnalysisResult.ForIssue(new ValidationIssue(
                ValidationDiagnostics.UnsupportedSelector,
                syntax.GetLocation(),
                value));
        }

        private static RuleAnalysisResult UnsupportedArgument(SyntaxNode syntax, string value)
        {
            return RuleAnalysisResult.ForIssue(new ValidationIssue(
                ValidationDiagnostics.UnsupportedArgument,
                syntax.GetLocation(),
                value));
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
