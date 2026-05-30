using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TinyValidations.Tests;

public sealed class RuntimeTests
{
    [Fact]
    public void Validation_error_stores_member_and_message()
    {
        var error = new ValidationError("Email", "Email is required.");

        Assert.Equal("Email", error.Member);
        Assert.Equal("Email is required.", error.Message);
    }

    [Theory]
    [InlineData(null, "Email is required.")]
    [InlineData("", "Email is required.")]
    [InlineData("   ", "Email is required.")]
    [InlineData("Email", null)]
    [InlineData("Email", "")]
    [InlineData("Email", "   ")]
    public void Validation_error_rejects_missing_member_or_message(string? member, string? message)
    {
        Assert.Throws<ArgumentException>(() => new ValidationError(member!, message!));
    }

    [Fact]
    public void Empty_error_collection_returns_valid_result()
    {
        var errors = new ValidationErrorCollection();

        var result = errors.ToResult();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Error_collection_returns_invalid_result_when_errors_exist()
    {
        var errors = new ValidationErrorCollection();

        errors.Add("Email", "Email is required.");

        var result = errors.ToResult();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void Error_collection_rejects_null_ranges()
    {
        var errors = new ValidationErrorCollection();

        Assert.Throws<ArgumentNullException>(() => errors.AddRange(null!));
    }

    [Fact]
    public void Invalid_result_uses_error_snapshot()
    {
        var errors = new List<ValidationError>
        {
            new ValidationError("Email", "Email is required.")
        };

        var result = new ValidationResult(errors);
        errors.Add(new ValidationError("Name", "Name is required."));

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void Validation_result_rejects_null_error_collection()
    {
        Assert.Throws<ArgumentNullException>(() => new ValidationResult(null!));
    }

    [Fact]
    public async Task Validator_returns_valid_result_when_no_runner_is_registered()
    {
        var validator = BuildValidator();

        var result = await validator.ValidateAsync(new CommandWithoutValidation());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validator_rejects_null_instances()
    {
        var validator = BuildValidator();

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await validator.ValidateAsync<CommandWithoutValidation>(null!));
    }

    [Fact]
    public void Use_tiny_validations_registers_validator_once()
    {
        var services = new ServiceCollection();

        services.UseTinyValidations();
        services.UseTinyValidations();

        var count = CountValidatorRegistrations(services);

        Assert.Equal(1, count);
    }

    [Fact]
    public void Use_tiny_validations_rejects_null_services()
    {
        Assert.Throws<ArgumentNullException>(() => TinyValidationServiceCollectionExtensions.UseTinyValidations(null!));
    }

    [Fact]
    public void Bootstrap_apply_rejects_null_services()
    {
        Assert.Throws<ArgumentNullException>(() => TinyValidationBootstrap.Apply(null!));
    }

    [Fact]
    public void Use_tiny_validations_can_be_called_twice_without_duplicate_runner_registrations()
    {
        var services = new ServiceCollection();

        services.UseTinyValidations();
        services.UseTinyValidations();

        var count = CountRegistrations<ITinyValidationRunner<CreateProfile>>(services);

        Assert.Equal(1, count);
    }

    [Fact]
    public void Bootstrap_can_apply_contributions_to_multiple_service_collections()
    {
        var first = new ServiceCollection();
        var second = new ServiceCollection();

        first.UseTinyValidations();
        second.UseTinyValidations();

        var firstCount = CountRegistrations<ITinyValidationRunner<CreateProfile>>(first);
        var secondCount = CountRegistrations<ITinyValidationRunner<CreateProfile>>(second);

        Assert.Equal(1, firstCount);
        Assert.Equal(1, secondCount);
    }

    [Fact]
    public void Duplicate_contribution_does_not_register_duplicate_services()
    {
        var services = new ServiceCollection();

        TinyValidationBootstrap.AddContribution(new DuplicateTestContribution());
        TinyValidationBootstrap.AddContribution(new DuplicateTestContribution());
        services.UseTinyValidations();

        var count = CountRegistrations<DuplicateContributionMarker>(services);

        Assert.Equal(1, count);
    }

    private static ITinyValidator BuildValidator()
    {
        var services = new ServiceCollection();

        services.UseTinyValidations();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ITinyValidator>();
    }

    private static int CountValidatorRegistrations(IServiceCollection services)
    {
        return CountRegistrations<ITinyValidator>(services);
    }

    private static int CountRegistrations<TService>(IServiceCollection services)
    {
        return CountRegistrations(services, typeof(TService));
    }

    private static int CountRegistrations(IServiceCollection services, Type serviceType)
    {
        var count = 0;

        foreach (var service in services)
        {
            if (service.ServiceType != serviceType)
            {
                continue;
            }

            count++;
        }

        return count;
    }
}

public sealed record CommandWithoutValidation;

public sealed class DuplicateContributionMarker
{
}

public sealed class DuplicateTestContribution : ITinyValidationContribution
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<DuplicateContributionMarker>();
    }
}
