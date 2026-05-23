using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Emission.Writing;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal static class RuleMessage
    {
        public static string For(RuleDefinition rule, string defaultMessage)
        {
            return string.IsNullOrWhiteSpace(rule.Message)
                ? StringLiteral.Create(defaultMessage)
                : rule.Message;
        }
    }
}

