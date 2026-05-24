using System.Collections.Generic;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.Rules
{
    internal sealed class RuleMethodMap
    {
        private readonly Dictionary<string, RuleKind> _rules = new Dictionary<string, RuleKind>
        {
            ["Required"] = RuleKind.Required,
            ["HasText"] = RuleKind.HasText,
            ["NotNull"] = RuleKind.NotNull,
            ["HasItems"] = RuleKind.HasItems,
            ["Email"] = RuleKind.Email,
            ["TextLengthAtLeast"] = RuleKind.TextLengthAtLeast,
            ["TextLengthAtMost"] = RuleKind.TextLengthAtMost,
            ["Above"] = RuleKind.Above,
            ["AtLeast"] = RuleKind.AtLeast,
            ["Below"] = RuleKind.Below,
            ["AtMost"] = RuleKind.AtMost,
            ["Matches"] = RuleKind.Matches,
            ["Requires"] = RuleKind.Requires,
            ["Use"] = RuleKind.Use
        };

        public RuleKind? GetKind(string methodName)
        {
            if (_rules.TryGetValue(methodName, out var kind))
            {
                return kind;
            }

            return null;
        }
    }
}
