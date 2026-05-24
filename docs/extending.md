# Extending TinyValidations

TinyValidations has two extension paths.

Use async custom rules when validation needs services or business state. Use requirement rules when validation is a small static check that should be emitted by the source generator.

## Async Custom Rules

Use `IAsyncValidationRule<T>` for validation that needs dependency injection, scoped services, database checks, or multiple fields.

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

Use it from a validation declaration:

```csharp
rules.Use<UniqueEmailRule>();
```

## Requirement Rules

Use `Requires` for small reusable checks that do not need services.

```csharp
public sealed class CreateOrderValidation : IValidation<CreateOrder>
{
    public void Define(ValidationRules<CreateOrder> rules)
    {
        rules.Requires(
            x => x.OrderNumber,
            OrderNumberRequirements.HasOrderPrefix,
            "Order number must start with ORD-.");
    }
}
```

The requirement method must be static, return `bool`, and accept the selected member value:

```csharp
public static class OrderNumberRequirements
{
    public static bool HasOrderPrefix(string? value)
    {
        return value is not null &&
            value.StartsWith("ORD-", StringComparison.Ordinal);
    }
}
```

TinyValidations analyzes the rule call shape, not the requirement method body. The generator emits a direct static method call:

```csharp
if (!OrderNumberRequirements.HasOrderPrefix(instance.OrderNumber))
{
    errors.Add("OrderNumber", "Order number must start with ORD-.");
}
```

## Requirement Rule Requirements

- The member selector must be a simple member access, such as `x => x.OrderNumber`.
- The requirement must be a static method group.
- The requirement must return `bool`.
- The requirement must have exactly one parameter.
- The message must be a string literal.

## Choosing The Extension Point

Use `Requires` for reusable deterministic checks:

- prefix checks
- checksum checks
- domain-specific format checks
- simple value-object conventions

Use `IAsyncValidationRule<T>` for application rules:

- uniqueness checks
- authorization or policy checks
- database-backed validation
- checks that need scoped services
- checks that inspect more than one field
