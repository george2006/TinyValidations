using Identity;
using Microsoft.Extensions.DependencyInjection;
using Sales;
using Xunit;

namespace TinyValidations.Tests;

public sealed class MultiAssemblyContributionTests
{
    [Fact]
    public async Task Host_registration_applies_contributions_from_referenced_libraries()
    {
        var services = new ServiceCollection();

        services.AddIdentitySample();
        services.AddSalesSample();
        services.UseTinyValidations();

        var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<ITinyValidator>();

        var registerUser = await validator.ValidateAsync(new RegisterUser("taken@example.com", "A", 15));
        var placeOrder = await validator.ValidateAsync(new PlaceOrder("blocked@example.com", Array.Empty<OrderLine>(), 0));

        AssertHasError(registerUser, nameof(RegisterUser.Email), "Email is already registered.");
        AssertHasError(placeOrder, nameof(PlaceOrder.CustomerEmail), "Customer cannot place orders.");
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
}
