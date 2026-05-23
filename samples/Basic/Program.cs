using Microsoft.Extensions.DependencyInjection;
using TinyValidations;

var services = new ServiceCollection();
services.AddScoped<UserStore>();
services.UseTinyValidations();

var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<ITinyValidator>();

var result = await validator.ValidateAsync(new CreateUser("bad-email", "J", 17));

foreach (var error in result.Errors)
{
    Console.WriteLine($"{error.Member}: {error.Message}");
}

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

public sealed class UniqueEmailRule : IAsyncValidationRule<CreateUser>
{
    private readonly UserStore _users;

    public UniqueEmailRule(UserStore users)
    {
        _users = users;
    }

    public ValueTask ValidateAsync(CreateUser instance, ValidationErrorCollection errors, CancellationToken cancellationToken)
    {
        if (_users.Exists(instance.Email))
        {
            errors.Add(nameof(CreateUser.Email), "Email is already registered.");
        }

        return ValueTask.CompletedTask;
    }
}

public sealed class UserStore
{
    public bool Exists(string email) => email == "existing@example.com";
}

public sealed record CreateUser(string Email, string Name, int Age);
