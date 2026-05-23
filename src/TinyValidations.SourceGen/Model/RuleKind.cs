namespace TinyValidations.SourceGen.Model
{
    internal enum RuleKind
    {
        Required,
        HasText,
        NotNull,
        HasItems,
        Email,
        TextLengthAtLeast,
        TextLengthAtMost,
        Above,
        AtLeast,
        Below,
        AtMost,
        Matches,
        Use
    }
}
