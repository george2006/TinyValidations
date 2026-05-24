using TinyDispatcher;
using TinyDispatcher.Dispatching;
using TinyDispatcherAspNetCore;
using TinyDispatcherAspNetCore.Validation;
using TinyValidations;
using TinyDispatcherAppContext = TinyDispatcher.AppContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<UserStore>();

builder.Services.UseTinyValidations();
builder.Services.AddTransient(typeof(TinyValidationMiddleware<>));

builder.Services.UseTinyDispatcher<TinyDispatcherAppContext>(tiny =>
{
    tiny.UseGlobalMiddleware(typeof(TinyValidationMiddleware<>));
});

var app = builder.Build();

app.UseMiddleware<TinyValidationProblemDetailsMiddleware>();

app.MapGet("/", () => Results.Ok(new { name = "TinyValidations TinyDispatcher ASP.NET sample" }));

app.MapPost("/users", async (
    CreateUserRequest request,
    IDispatcher<TinyDispatcherAppContext> dispatcher,
    CancellationToken cancellationToken) =>
{
    var command = new CreateUser(request.Email, request.Name, request.Age);

    await dispatcher.DispatchAsync(command, cancellationToken);

    return Results.Created($"/users/{Uri.EscapeDataString(command.Email)}", new
    {
        command.Email,
        command.Name,
        command.Age
    });
});

app.Run();

public sealed record CreateUserRequest(string Email, string Name, int Age);
