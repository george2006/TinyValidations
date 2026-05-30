using System;
using System.Collections.Generic;

namespace TinyValidations;

public sealed class ValidationErrorCollection
{
    private readonly List<ValidationError> _errors = new List<ValidationError>();

    public IReadOnlyCollection<ValidationError> Errors => _errors;

    public void Add(string member, string message)
    {
        _errors.Add(new ValidationError(member, message));
    }

    public void AddRange(IEnumerable<ValidationError> errors)
    {
        if (errors is null)
        {
            throw new ArgumentNullException(nameof(errors));
        }

        _errors.AddRange(errors);
    }

    public ValidationResult ToResult()
    {
        if (_errors.Count == 0)
        {
            return ValidationResult.Valid;
        }

        return new ValidationResult(_errors.ToArray());
    }
}
