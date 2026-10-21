# Getting Started

This guide shows the basic TinyValidations flow.

## Install

Install TinyValidations:

```bash
dotnet add package TinyValidations
```

For local development, reference the project directly.

## Register Services

TinyValidations uses Microsoft dependency injection.

```csharp
using Microsoft.Extensions.DependencyInjection;
using TinyValidations;

var services = new ServiceCollection();

services.UseTinyValidations();
```

Calling `UseTinyValidations` more than once on the same service collection is safe.

Register any services required by custom rules before building the provider:

```csharp
services.AddScoped<UserStore>();
```

## Define A Command

```csharp
public sealed record CreateUser(string Email, string Name, int Age);
```

## Define Validation

```csharp
public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Required(x => x.Email);
        rules.Email(x => x.Email);
        rules.TextLengthAtLeast(x => x.Name, 2);
        rules.AtLeast(x => x.Age, 18);
    }
}
```

## Check Declarations

The generator reads `Define` at compile time; it does not execute that method. Values and supplied
messages in built-in rules must be compile-time constants. You can use `const` fields or locals
instead of literals. See [Argument forms](rules.md#argument-forms).

Unsupported declarations produce compilation errors `TV0001` through `TV0006`. The generator
reports issues across declarations and continues generating valid declarations, but all errors
must be resolved before the build succeeds.

## Run Validation

```csharp
var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<ITinyValidator>();

var command = new CreateUser("bad-email", "J", 17);
var result = await validator.ValidateAsync(command);
```

## Read Errors

```csharp
if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.Member}: {error.Message}");
    }
}
```

## Next

- [Rules](rules.md)
- [Custom rules](custom-rules.md)
- [Extending TinyValidations](extending.md)
- [TinyDispatcher integration](tinydispatcher.md)
