using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.Rules
{
    internal sealed class RuleAnalysisResult
    {
        private RuleAnalysisResult(RuleDefinition? rule, ValidationIssue? issue)
        {
            Rule = rule;
            Issue = issue;
        }

        public RuleDefinition? Rule { get; }

        public ValidationIssue? Issue { get; }

        public static RuleAnalysisResult ForRule(RuleDefinition rule)
        {
            return new RuleAnalysisResult(rule, null);
        }

        public static RuleAnalysisResult ForIssue(ValidationIssue issue)
        {
            return new RuleAnalysisResult(null, issue);
        }
    }
}
