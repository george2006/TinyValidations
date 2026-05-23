# Custom Rules

Custom rules are for validation that cannot be expressed as a small built-in check.

Use them for:

- uniqueness checks
- database-backed validation
- policy checks
- validation that needs scoped services
- validation that depends on multiple fields

## Define A Rule

```csharp
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
```

## Use The Rule

```csharp
public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Use<UniqueEmailRule>();
    }
}
```

## Dependency Injection

Custom rules are registered by generated contributions.

Services used by custom rules must be registered by the application:

```csharp
services.AddScoped<UserStore>();
services.UseTinyValidations();
```

## Error Collection

Custom rules add errors through `ValidationErrorCollection`.

```csharp
errors.Add(nameof(CreateUser.Email), "Email is already registered.");
```

The first argument is the member name. The second argument is the user-facing message.
