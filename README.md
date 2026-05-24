# TinyValidations

TinyValidations is a small compile-time validation library for application-layer commands.

It was built for TinyDispatcher-style applications first: define a command, define its validation, register services, and let generated code do the boring work before the handler runs. Native ASP.NET integration is planned, but the core package is intentionally host-agnostic.

> Status: experimental alpha. The public shape is small, but diagnostics, packaging, and host integrations are still evolving.

## Contents

- [Why TinyValidations?](#why-tinyvalidations)
- [Quick start](#quick-start)
- [Validation declarations](#validation-declarations)
- [Custom rules](#custom-rules)
- [TinyDispatcher](#tinydispatcher)
- [Documentation](#documentation)
- [Current limitations](#current-limitations)

## Why TinyValidations?

Most validation libraries do their work at runtime.

TinyValidations keeps the developer API small and moves the repetitive validation runner code to a source generator. You declare intent with `IValidation<T>` and `ValidationRules<T>`. The generator emits ordinary C# runners that are registered through Microsoft dependency injection.

The goal is not to be a huge validation framework. The goal is calm command validation:

- no reflection-based rule discovery at runtime
- no runtime expression parsing for built-in rules
- direct generated checks for common rules
- generator-native requirement rules for simple application-specific checks
- DI-friendly custom async rules for real business checks
- a small API that fits application-layer code

## Quick start

Install the package, then register TinyValidations:

```csharp
using Microsoft.Extensions.DependencyInjection;
using TinyValidations;

var services = new ServiceCollection();

services.AddScoped<UserStore>();
services.UseTinyValidations();

var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<ITinyValidator>();

var result = await validator.ValidateAsync(new CreateUser("bad-email", "J", 17));
```

Define a validation for your command:

```csharp
public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Required(x => x.Email);
        rules.Email(x => x.Email);
        rules.TextLengthAtLeast(x => x.Name, 2);
        rules.AtLeast(x => x.Age, 18);
        rules.Use<UniqueEmailRule>();
    }
}

public sealed record CreateUser(string Email, string Name, int Age);
```

Read validation errors:

```csharp
foreach (var error in result.Errors)
{
    Console.WriteLine($"{error.Member}: {error.Message}");
}
```

See the full sample in [`samples/Basic`](samples/Basic/Program.cs).

## Validation declarations

A validation declaration implements `IValidation<T>`:

```csharp
public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Required(x => x.Email);
    }
}
```

The `Define` method is declaration-only. TinyValidations reads it at compile time and emits generated validation runners.

Supported built-in rules are documented in [Rules](docs/rules.md).

## Custom rules

Custom rules implement `IAsyncValidationRule<T>`. They are resolved from dependency injection, so they can use scoped services.

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

More detail is in [Custom rules](docs/custom-rules.md).

For static generator-native checks, use `Requires`:

```csharp
rules.Requires(
    x => x.OrderNumber,
    OrderNumberRequirements.HasOrderPrefix,
    "Order number must start with ORD-.");
```

Read more in [Extending TinyValidations](docs/extending.md).

## TinyDispatcher

TinyValidations is designed to become the validation companion for TinyDispatcher.

The intended flow is:

1. A command enters the dispatcher.
2. TinyValidations validates the command.
3. The dispatcher stops when validation fails.
4. The command handler runs only for valid commands.

The current package exposes the validation primitives. The TinyDispatcher integration package is planned separately so the core remains useful outside TinyDispatcher.

Read more in [TinyDispatcher integration](docs/tinydispatcher.md).

See [`samples/TinyDispatcherAspNetCore`](samples/TinyDispatcherAspNetCore) for a minimal ASP.NET sample that wires TinyValidations into TinyDispatcher middleware and maps validation failures to problem details.

See [`samples/MediatRAspNetCore`](samples/MediatRAspNetCore) for the same application shape using a MediatR pipeline behavior.

## Documentation

- [Getting started](docs/getting-started.md)
- [Rules](docs/rules.md)
- [Custom rules](docs/custom-rules.md)
- [Extending TinyValidations](docs/extending.md)
- [TinyDispatcher integration](docs/tinydispatcher.md)
- [MediatR integration](docs/mediatr.md)
- [Architecture](docs/architecture.md)
- [Roadmap](docs/roadmap.md)

## Current limitations

TinyValidations is intentionally small and currently early.

- Source generator diagnostics are minimal.
- Supported rule declaration shapes are narrow.
- Generated registration uses the current bootstrap mechanism and may change.
- Native ASP.NET integration is planned but not implemented yet.
- Tests cover runtime behavior, generated behavior, diagnostics, and multi-assembly contributions.

See [Roadmap](docs/roadmap.md) for the next planned steps.
