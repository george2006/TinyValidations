using TinyValidations;

namespace TinyDispatcherAspNetCore.Validation;

public sealed class TinyValidationException : Exception
{
    public TinyValidationException(IReadOnlyCollection<ValidationError> errors)
        : base("The request failed validation.")
    {
        Errors = errors;
    }

    public IReadOnlyCollection<ValidationError> Errors { get; }
}
