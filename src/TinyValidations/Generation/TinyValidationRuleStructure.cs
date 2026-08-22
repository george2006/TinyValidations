using System;

namespace TinyValidations;

public sealed class TinyValidationRuleStructure
{
    public TinyValidationRuleStructure(string memberPath, string kind)
    {
        if (string.IsNullOrWhiteSpace(memberPath))
        {
            throw new ArgumentException("Member path is required.", nameof(memberPath));
        }

        if (string.IsNullOrWhiteSpace(kind))
        {
            throw new ArgumentException("Rule kind is required.", nameof(kind));
        }

        MemberPath = memberPath;
        Kind = kind;
    }

    public string MemberPath { get; }

    public string Kind { get; }
}
