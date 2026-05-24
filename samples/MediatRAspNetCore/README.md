# MediatR ASP.NET Sample

This sample shows TinyValidations running inside a MediatR pipeline behavior and returning ASP.NET validation problem details.

The sample keeps the integration code local on purpose:

- `TinyValidationBehavior<TRequest, TResponse>` validates requests before handlers run.
- `TinyValidationException` carries validation failures out of the MediatR pipeline.
- `TinyValidationProblemDetailsMiddleware` converts those failures to `application/problem+json`.

## Run

```bash
dotnet run --project samples/MediatRAspNetCore/MediatRAspNetCore.csproj
```

Send an invalid request:

```http
POST /users
Content-Type: application/json

{
  "email": "bad-email",
  "name": "J",
  "age": 17
}
```

The response is a `400` validation problem details payload.
