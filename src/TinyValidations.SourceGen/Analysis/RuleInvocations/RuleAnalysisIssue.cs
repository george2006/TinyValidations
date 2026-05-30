using Microsoft.CodeAnalysis;
using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Validation;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal static class RuleAnalysisIssue
    {
        public static RuleAnalysisResult UnsupportedRuleCall(SyntaxNode syntax, string value)
        {
            return Create(ValidationDiagnostics.UnsupportedRuleCall, syntax, value);
        }

        public static RuleAnalysisResult UnsupportedSelector(SyntaxNode syntax, string value)
        {
            return Create(ValidationDiagnostics.UnsupportedSelector, syntax, value);
        }

        public static RuleAnalysisResult UnsupportedArgument(SyntaxNode syntax, string value)
        {
            return Create(ValidationDiagnostics.UnsupportedArgument, syntax, value);
        }

        public static RuleAnalysisResult InvalidCustomRule(SyntaxNode syntax, string value)
        {
            return Create(ValidationDiagnostics.InvalidCustomRule, syntax, value);
        }

        private static RuleAnalysisResult Create(
            DiagnosticDescriptor descriptor,
            SyntaxNode syntax,
            string value)
        {
            return RuleAnalysisResult.ForIssue(new ValidationIssue(
                descriptor,
                syntax.GetLocation(),
                value));
        }
    }
}
