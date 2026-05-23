namespace TinyValidations;

public sealed class ValidationError
{
    public ValidationError(string member, string message)
    {
        Member = member;
        Message = message;
    }

    public string Member { get; }

    public string Message { get; }
}
