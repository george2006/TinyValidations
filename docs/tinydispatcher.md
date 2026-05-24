# TinyDispatcher Integration

TinyValidations is being built as the validation companion for TinyDispatcher.

The core package does not require TinyDispatcher. That is intentional. The validation engine should stay small, testable, and useful in plain DI applications too.

## Intended Flow

The planned TinyDispatcher integration will validate commands before handlers run.

```text
Command
  -> TinyDispatcher
  -> TinyValidations
  -> Handler
```

If validation fails, the handler should not run.

## Current Usage

Today, use `ITinyValidator` directly:

```csharp
var result = await validator.ValidateAsync(command, cancellationToken);
```

A TinyDispatcher behavior or middleware package can call this before invoking the handler.

The ASP.NET sample in `samples/TinyDispatcherAspNetCore` shows the intended shape using TinyDispatcher `1.1.0`:

```csharp
services.UseTinyValidations();
services.AddTransient(typeof(TinyValidationMiddleware<>));

services.UseTinyDispatcher<AppContext>(tiny =>
{
    tiny.UseGlobalMiddleware(typeof(TinyValidationMiddleware<>));
});
```

The sample middleware throws a custom validation exception when validation fails. An ASP.NET middleware catches that exception and writes a standard validation problem details response.

## Planned Package Shape

The likely package split is:

```text
TinyValidations
TinyValidations.TinyDispatcher
TinyValidations.AspNetCore
```

`TinyValidations` remains the host-agnostic core.

`TinyValidations.TinyDispatcher` will provide native dispatcher integration.

`TinyValidations.AspNetCore` will provide native ASP.NET integration later.

## Design Goal

TinyDispatcher users should only need to:

```csharp
services.UseTinyDispatcher();
services.UseTinyValidations();
```

Then command validation should happen naturally in the dispatch pipeline.
