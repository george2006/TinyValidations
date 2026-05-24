using MediatR;
using MediatRAspNetCore;
using MediatRAspNetCore.Validation;
using TinyValidations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<UserStore>();

builder.Services.UseTinyValidations();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TinyValidationBehavior<,>));

builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssemblyContaining<Program>();
});

var app = builder.Build();

app.UseMiddleware<TinyValidationProblemDetailsMiddleware>();

app.MapGet("/", () => Results.Ok(new { name = "TinyValidations MediatR ASP.NET sample" }));

app.MapPost("/users", async (
    CreateUserRequest request,
    ISender sender,
    CancellationToken cancellationToken) =>
{
    var command = new CreateUser(request.Email, request.Name, request.Age);
    var result = await sender.Send(command, cancellationToken);

    return Results.Created($"/users/{Uri.EscapeDataString(result.Email)}", result);
});

app.Run();

public sealed record CreateUserRequest(string Email, string Name, int Age);
