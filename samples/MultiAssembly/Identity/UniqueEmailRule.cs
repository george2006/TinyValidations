using TinyValidations;

namespace Identity;

public sealed class UniqueEmailRule : IAsyncValidationRule<RegisterUser>
{
    private readonly UserDirectory _users;

    public UniqueEmailRule(UserDirectory users)
    {
        _users = users;
    }

    public ValueTask ValidateAsync(
        RegisterUser instance,
        ValidationErrorCollection errors,
        CancellationToken cancellationToken)
    {
        if (_users.EmailExists(instance.Email))
        {
            errors.Add(nameof(RegisterUser.Email), "Email is already registered.");
        }

        return ValueTask.CompletedTask;
    }
}
