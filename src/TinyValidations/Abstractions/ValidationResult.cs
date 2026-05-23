using System.Collections.Generic;
using System.Linq;

namespace TinyValidations;

public sealed class ValidationResult
{
    public static readonly ValidationResult Valid = new ValidationResult(System.Array.Empty<ValidationError>());

    public ValidationResult(IReadOnlyCollection<ValidationError> errors)
    {
        Errors = errors;
    }

    public IReadOnlyCollection<ValidationError> Errors { get; }

    public bool IsValid => !Errors.Any();
}
