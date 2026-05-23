namespace TinyValidations.SourceGen.Model
{
    internal sealed class RuleDefinition
    {
        public RuleDefinition(
            RuleKind kind,
            string memberPath,
            string memberAccess,
            string argument,
            string message,
            string customRuleType)
        {
            Kind = kind;
            MemberPath = memberPath;
            MemberAccess = memberAccess;
            Argument = argument;
            Message = message;
            CustomRuleType = customRuleType;
        }

        public RuleKind Kind { get; }

        public string MemberPath { get; }

        public string MemberAccess { get; }

        public string Argument { get; }

        public string Message { get; }

        public string CustomRuleType { get; }
    }
}
