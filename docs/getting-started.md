# Getting Started

This guide shows the basic TinyValidations flow.

## Install

TinyValidations is currently published as a beta package:

```bash
dotnet add package TinyValidations --version 0.1.0-beta.1
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
