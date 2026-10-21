using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TinyValidations.Tests;

public sealed class ConstantTextArgumentBehaviorTests
{
    [Fact]
    public async Task Returns_resolved_messages_with_their_original_text()
    {
        var services = new ServiceCollection();
        services.UseTinyValidations();

        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<ITinyValidator>();
        var command = new ConstantTextArgumentsCommand("", "abc", "invalid");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Collection(result.Errors,
            error =>
            {
                Assert.Equal("RequiredText", error.Member);
                Assert.Equal("Enter \"text\".\nTry again.", error.Message);
            },
            error =>
            {
                Assert.Equal("Code", error.Member);
                Assert.Equal(@"Code must match \d{3}.", error.Message);
            },
            error =>
            {
                Assert.Equal("RequirementText", error.Member);
                Assert.Equal("RequirementText must start with OK-.", error.Message);
            });
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("abc", false)]
    [InlineData("1234", false)]
    [InlineData("", true)]
    public async Task Matches_applies_the_resolved_pattern_and_preserves_empty_value_behavior(string code, bool expectedValid)
    {
        var services = new ServiceCollection();
        services.UseTinyValidations();

        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<ITinyValidator>();
        var command = new ConstantTextArgumentsCommand("text", code, "OK-123");

        var result = await validator.ValidateAsync(command);

        Assert.Equal(expectedValid, result.IsValid);

        if (expectedValid)
        {
            Assert.Empty(result.Errors);
            return;
        }

        var error = Assert.Single(result.Errors);

        Assert.Equal("Code", error.Member);
        Assert.Equal(@"Code must match \d{3}.", error.Message);
    }
}

public sealed record ConstantTextArgumentsCommand(string RequiredText, string Code, string RequirementText);

public sealed class ConstantTextArgumentsCommandValidation : IValidation<ConstantTextArgumentsCommand>
{
    private const string RequiredMessage = "Enter \"text\".\nTry again.";
    private const string CodePattern = @"^\d{3}$";
    private const string CodeMessage = @"Code must match \d{3}.";

    public void Define(ValidationRules<ConstantTextArgumentsCommand> rules)
    {
        const string requirementMessage = nameof(ConstantTextArgumentsCommand.RequirementText) + " must start with OK-.";

        rules.Required(x => x.RequiredText, RequiredMessage);
        rules.Matches(x => x.Code, CodePattern, CodeMessage);
        rules.Requires(x => x.RequirementText, ConstantTextRequirements.HasPrefix, requirementMessage);
    }
}

public static class ConstantTextRequirements
{
    public static bool HasPrefix(string value)
    {
        return value.StartsWith("OK-", StringComparison.Ordinal);
    }
}
