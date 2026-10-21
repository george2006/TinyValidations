using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TinyValidations.Tests;

public sealed class ConstantArgumentBehaviorTests
{
    [Theory]
    [InlineData("ab", "abcd", "Minimum", "Minimum must contain at least 3 characters.")]
    [InlineData("abc", "abcde", null, null)]
    [InlineData("abcd", "abcdef", "Maximum", "Maximum must contain at most 5 characters.")]
    public async Task Text_length_rules_apply_constant_limits(
        string minimum,
        string maximum,
        string? expectedMember,
        string? expectedMessage)
    {
        var services = new ServiceCollection();
        services.UseTinyValidations();

        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<ITinyValidator>();
        var command = new ConstantTextLengthCommand(minimum, maximum);

        var result = await validator.ValidateAsync(command);

        if (expectedMember == null)
        {
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            return;
        }

        Assert.False(result.IsValid);

        var error = Assert.Single(result.Errors);

        Assert.Equal(expectedMember, error.Member);
        Assert.Equal(expectedMessage, error.Message);
    }

    [Theory]
    [InlineData("1.234567890123456788", "Above", "AtLeast")]
    [InlineData("1.234567890123456789", "Above", "Below")]
    [InlineData("1.234567890123456790", "Below", "AtMost")]
    public async Task Comparison_rules_preserve_decimal_constant_precision(string valueText, params string[] expectedMembers)
    {
        var services = new ServiceCollection();
        services.UseTinyValidations();

        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<ITinyValidator>();
        var value = decimal.Parse(valueText, CultureInfo.InvariantCulture);
        var command = new ConstantComparisonCommand(value, value, value, value);

        var result = await validator.ValidateAsync(command);
        var members = result.Errors.Select(error => error.Member).ToArray();

        Assert.False(result.IsValid);
        Assert.Equal(expectedMembers, members);
    }
}

public sealed record ConstantTextLengthCommand(string Minimum, string Maximum);

public sealed class ConstantTextLengthCommandValidation : IValidation<ConstantTextLengthCommand>
{
    private const int MinimumLength = 3;

    public void Define(ValidationRules<ConstantTextLengthCommand> rules)
    {
        const int maximumLength = MinimumLength + 2;

        rules.TextLengthAtLeast(x => x.Minimum, MinimumLength);
        rules.TextLengthAtMost(x => x.Maximum, maximumLength);
    }
}

public sealed record ConstantComparisonCommand(decimal Above, decimal AtLeast, decimal Below, decimal AtMost)
{
    public const decimal Threshold = 1.234567890123456789m;
}

public sealed class ConstantComparisonCommandValidation : IValidation<ConstantComparisonCommand>
{
    public void Define(ValidationRules<ConstantComparisonCommand> rules)
    {
        rules.Above(x => x.Above, ConstantComparisonCommand.Threshold);
        rules.AtLeast(x => x.AtLeast, ConstantComparisonCommand.Threshold);
        rules.Below(x => x.Below, ConstantComparisonCommand.Threshold);
        rules.AtMost(x => x.AtMost, ConstantComparisonCommand.Threshold);
    }
}
