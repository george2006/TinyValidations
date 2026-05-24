using TinyValidations;

namespace TinyDispatcherAspNetCore;

public sealed class UniqueEmailRule : IAsyncValidationRule<CreateUser>
{
    private readonly UserStore _users;

    public UniqueEmailRule(UserStore users)
    {
        _users = users;
    }

    public ValueTask ValidateAsync(
        CreateUser instance,
        ValidationErrorCollection errors,
        CancellationToken cancellationToken)
    {
        if (_users.Exists(instance.Email))
        {
            errors.Add(nameof(CreateUser.Email), "Email is already registered.");
        }

        return ValueTask.CompletedTask;
    }
}
