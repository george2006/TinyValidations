using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TinyValidations.Tests;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Built_in_rules_return_validation_errors()
    {
        var validator = BuildValidator();
        var command = new CreateProfile(
            "bad-email",
            "A",
            17,
            "abc",
            Array.Empty<string>());

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        AssertHasError(result, nameof(CreateProfile.Email), "Email must be a valid email address.");
        AssertHasError(result, nameof(CreateProfile.DisplayName), "DisplayName must contain at least 2 characters.");
        AssertHasError(result, nameof(CreateProfile.Age), "Age must be at least 18.");
        AssertHasError(result, nameof(CreateProfile.Code), "Code has an invalid format.");
        AssertHasError(result, nameof(CreateProfile.Roles), "Roles must contain at least one item.");
    }

    [Fact]
    public async Task Built_in_rules_use_custom_messages()
    {
        var validator = BuildValidator();
        var command = new CreateProfileWithCustomMessages(
            string.Empty,
            "A",
            17,
            "abc",
            Array.Empty<string>());

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        AssertHasError(result, nameof(CreateProfileWithCustomMessages.Email), "Please provide an email.");
        AssertHasError(result, nameof(CreateProfileWithCustomMessages.DisplayName), "Display name is too short.");
        AssertHasError(result, nameof(CreateProfileWithCustomMessages.Age), "Adults only.");
        AssertHasError(result, nameof(CreateProfileWithCustomMessages.Code), "Code must be three uppercase letters.");
        AssertHasError(result, nameof(CreateProfileWithCustomMessages.Roles), "Choose at least one role.");
    }

    [Fact]
    public async Task Custom_rules_are_resolved_from_dependency_injection()
    {
        var validator = BuildValidator(services => services.AddScoped<ReservedTeamNameStore>());
        var command = new CreateTeam("admin");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        AssertHasError(result, nameof(CreateTeam.Name), "Team name is reserved.");
    }

    [Fact]
    public async Task Multiple_validations_for_the_same_command_are_aggregated()
    {
        var validator = BuildValidator();
        var command = new ShipPackage(string.Empty, "bad");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        AssertHasError(result, nameof(ShipPackage.Address), "Address is required.");
        AssertHasError(result, nameof(ShipPackage.TrackingCode), "TrackingCode has an invalid format.");
    }

    [Fact]
    public async Task Requires_rules_call_static_requirement_methods()
    {
        var validator = BuildValidator();
        var command = new CreateOrder("bad");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        AssertHasError(result, nameof(CreateOrder.OrderNumber), "Order number must start with ORD-.");
    }

    [Fact]
    public async Task Registration_is_idempotent_for_generated_runners()
    {
        var services = new ServiceCollection();

        services.UseTinyValidations();
        services.UseTinyValidations();

        var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<ITinyValidator>();
        var command = new CreateProfile(
            "bad-email",
            "Valid Name",
            18,
            "ABC",
            new[] { "admin" });

        var result = await validator.ValidateAsync(command);

        Assert.Equal(1, CountErrors(result, nameof(CreateProfile.Email)));
    }

    [Fact]
    public async Task Custom_rules_use_scoped_dependencies()
    {
        var services = new ServiceCollection();

        services.AddScoped<ScopedRuleState>();
        services.UseTinyValidations();

        var provider = services.BuildServiceProvider();
        var firstMessage = await ValidateWithScopeAsync(provider);
        var secondMessage = await ValidateWithScopeAsync(provider);

        Assert.NotEqual(firstMessage, secondMessage);
    }

    private static ITinyValidator BuildValidator(Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();

        configureServices?.Invoke(services);
        services.UseTinyValidations();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ITinyValidator>();
    }

    private static void AssertHasError(ValidationResult result, string member, string message)
    {
        foreach (var error in result.Errors)
        {
            if (error.Member != member)
            {
                continue;
            }

            Assert.Equal(message, error.Message);
            return;
        }

        Assert.Fail("Expected validation error for " + member + ".");
    }

    private static int CountErrors(ValidationResult result, string member)
    {
        var count = 0;

        foreach (var error in result.Errors)
        {
            if (error.Member != member)
            {
                continue;
            }

            count++;
        }

        return count;
    }

    private static async Task<string> ValidateWithScopeAsync(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<ITinyValidator>();
        var result = await validator.ValidateAsync(new ScopedRuleCommand());

        foreach (var error in result.Errors)
        {
            if (error.Member != nameof(ScopedRuleCommand.Value))
            {
                continue;
            }

            return error.Message;
        }

        Assert.Fail("Expected scoped rule error.");
        return string.Empty;
    }
}

public sealed record CreateProfileWithCustomMessages(
    string Email,
    string DisplayName,
    int Age,
    string Code,
    IReadOnlyCollection<string> Roles);

public sealed class CreateProfileWithCustomMessagesValidation : IValidation<CreateProfileWithCustomMessages>
{
    public void Define(ValidationRules<CreateProfileWithCustomMessages> rules)
    {
        rules.Required(x => x.Email, "Please provide an email.");
        rules.TextLengthAtLeast(x => x.DisplayName, 2, "Display name is too short.");
        rules.AtLeast(x => x.Age, 18, "Adults only.");
        rules.Matches(x => x.Code, "^[A-Z]{3}$", "Code must be three uppercase letters.");
        rules.HasItems(x => x.Roles, "Choose at least one role.");
    }
}

public sealed record CreateProfile(
    string Email,
    string DisplayName,
    int Age,
    string Code,
    IReadOnlyCollection<string> Roles);

public sealed class CreateProfileValidation : IValidation<CreateProfile>
{
    public void Define(ValidationRules<CreateProfile> rules)
    {
        rules.Email(x => x.Email);
        rules.TextLengthAtLeast(x => x.DisplayName, 2);
        rules.AtLeast(x => x.Age, 18);
        rules.Matches(x => x.Code, "^[A-Z]{3}$");
        rules.HasItems(x => x.Roles);
    }
}

public sealed record CreateTeam(string Name);

public sealed class CreateTeamValidation : IValidation<CreateTeam>
{
    public void Define(ValidationRules<CreateTeam> rules)
    {
        rules.Required(x => x.Name);
        rules.Use<ReservedTeamNameRule>();
    }
}

public sealed class ReservedTeamNameRule : IAsyncValidationRule<CreateTeam>
{
    private readonly ReservedTeamNameStore _names;

    public ReservedTeamNameRule(ReservedTeamNameStore names)
    {
        _names = names;
    }

    public ValueTask ValidateAsync(
        CreateTeam instance,
        ValidationErrorCollection errors,
        CancellationToken cancellationToken)
    {
        if (_names.IsReserved(instance.Name))
        {
            errors.Add(nameof(CreateTeam.Name), "Team name is reserved.");
        }

        return ValueTask.CompletedTask;
    }
}

public sealed class ReservedTeamNameStore
{
    public bool IsReserved(string name)
    {
        return name == "admin";
    }
}

public sealed record ShipPackage(string Address, string TrackingCode);

public sealed class ShipPackageAddressValidation : IValidation<ShipPackage>
{
    public void Define(ValidationRules<ShipPackage> rules)
    {
        rules.Required(x => x.Address);
    }
}

public sealed class ShipPackageTrackingValidation : IValidation<ShipPackage>
{
    public void Define(ValidationRules<ShipPackage> rules)
    {
        rules.Matches(x => x.TrackingCode, "^[A-Z]{2}[0-9]{4}$");
    }
}

public sealed record CreateOrder(string OrderNumber);

public sealed class CreateOrderValidation : IValidation<CreateOrder>
{
    public void Define(ValidationRules<CreateOrder> rules)
    {
        rules.Requires(x => x.OrderNumber, OrderNumberRequirements.HasOrderPrefix, "Order number must start with ORD-.");
    }
}

public static class OrderNumberRequirements
{
    public static bool HasOrderPrefix(string? value)
    {
        return value is not null && value.StartsWith("ORD-", StringComparison.Ordinal);
    }
}

public sealed record ScopedRuleCommand(string Value = "");

public sealed class ScopedRuleCommandValidation : IValidation<ScopedRuleCommand>
{
    public void Define(ValidationRules<ScopedRuleCommand> rules)
    {
        rules.Use<ScopedDependencyRule>();
    }
}

public sealed class ScopedDependencyRule : IAsyncValidationRule<ScopedRuleCommand>
{
    private readonly ScopedRuleState _state;

    public ScopedDependencyRule(ScopedRuleState state)
    {
        _state = state;
    }

    public ValueTask ValidateAsync(
        ScopedRuleCommand instance,
        ValidationErrorCollection errors,
        CancellationToken cancellationToken)
    {
        errors.Add(nameof(ScopedRuleCommand.Value), _state.Id.ToString());
        return ValueTask.CompletedTask;
    }
}

public sealed class ScopedRuleState
{
    public Guid Id { get; } = Guid.NewGuid();
}
