using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal static class RuleShape
    {
        public static bool RequiresValueArgument(RuleKind kind)
        {
            return kind == RuleKind.TextLengthAtLeast
                || kind == RuleKind.TextLengthAtMost
                || kind == RuleKind.Above
                || kind == RuleKind.AtLeast
                || kind == RuleKind.Below
                || kind == RuleKind.AtMost
                || kind == RuleKind.Matches;
        }

        public static int ValueArgumentIndex(RuleKind kind)
        {
            if (RequiresValueArgument(kind))
            {
                return 1;
            }

            return -1;
        }

        public static int MessageArgumentIndex(RuleKind kind)
        {
            if (RequiresValueArgument(kind))
            {
                return 2;
            }

            return 1;
        }
    }
}
