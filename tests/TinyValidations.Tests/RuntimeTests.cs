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

    private static ITinyValidator BuildValidator()
    {
        var services = new ServiceCollection();

        services.UseTinyValidations();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ITinyValidator>();
    }

    private static int CountValidatorRegistrations(IServiceCollection services)
    {
        var count = 0;

        foreach (var service in services)
        {
            if (service.ServiceType != typeof(ITinyValidator))
            {
                continue;
            }

            count++;
        }

        return count;
    }
}

public sealed record CommandWithoutValidation;
