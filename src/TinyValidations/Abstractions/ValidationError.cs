using System;

namespace TinyValidations;

public sealed class ValidationError
{
    public ValidationError(string member, string message)
    {
        if (string.IsNullOrWhiteSpace(member))
        {
            throw new ArgumentException("Validation error member is required.", nameof(member));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Validation error message is required.", nameof(message));
        }

        Member = member;
        Message = message;
    }

    public string Member { get; }

    public string Message { get; }
}
