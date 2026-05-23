# MultiAssembly Sample

This sample checks that generated validation contributions work from more than one class library.

## Projects

- `Host`: console application
- `Identity`: class library with `RegisterUser` validation
- `Sales`: class library with `PlaceOrder` validation

The host references both class libraries and calls one `UseTinyValidations()` registration.

Each class library references the source generator as an analyzer because this sample uses project references instead of the packaged NuGet layout.

## Run

```bash
dotnet run --project samples/MultiAssembly/Host/Host.csproj
```

Expected output includes validation errors from both class libraries.
