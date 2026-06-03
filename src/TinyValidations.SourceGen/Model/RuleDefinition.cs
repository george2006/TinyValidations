namespace TinyValidations.SourceGen.Model
{
    internal sealed class RuleDefinition
    {
        public RuleDefinition(
            RuleKind kind,
            string memberPath,
            string memberAccess,
            string argument,
            string argumentDisplay,
            string message,
            string customRuleType,
            string requirementMethod = "",
            string comparisonTypeName = "")
        {
            Kind = kind;
            MemberPath = memberPath;
            MemberAccess = memberAccess;
            Argument = argument;
            ArgumentDisplay = argumentDisplay;
            Message = message;
            CustomRuleType = customRuleType;
            RequirementMethod = requirementMethod;
            ComparisonTypeName = comparisonTypeName;
        }

        public RuleKind Kind { get; }

        public string MemberPath { get; }

        public string MemberAccess { get; }

        public string Argument { get; }

        public string ArgumentDisplay { get; }

        public string Message { get; }

        public string CustomRuleType { get; }

        public string RequirementMethod { get; }

        public string ComparisonTypeName { get; }
    }
}
