# MediatR Integration

TinyValidations can run inside a MediatR pipeline behavior.

The core package does not require MediatR. That keeps TinyValidations small and host-agnostic.

## Intended Flow

```text
Request
  -> MediatR
  -> TinyValidations
  -> Handler
```

If validation fails, the handler should not run.

## Sample

The ASP.NET sample in `samples/MediatRAspNetCore` shows the integration shape:

```csharp
services.UseTinyValidations();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TinyValidationBehavior<,>));

services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssemblyContaining<Program>();
});
```

The behavior validates each request before calling the next handler delegate:

```csharp
var result = await validator.ValidateAsync(request, cancellationToken);

if (!result.IsValid)
{
    throw new TinyValidationException(result.Errors);
}

return await next();
```

In ASP.NET applications, a small middleware can catch that validation exception and write a standard validation problem details response.
