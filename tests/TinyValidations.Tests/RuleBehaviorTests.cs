using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TinyValidations.Tests;

public sealed class RuleBehaviorTests
{
    [Fact]
    public async Task Required_rejects_null_empty_and_whitespace_text()
    {
        var validator = BuildValidator();

        var nullResult = await validator.ValidateAsync(new RequiredRuleCommand(null));
        var emptyResult = await validator.ValidateAsync(new RequiredRuleCommand(string.Empty));
        var whitespaceResult = await validator.ValidateAsync(new RequiredRuleCommand("   "));
        var validResult = await validator.ValidateAsync(new RequiredRuleCommand("value"));

        AssertHasError(nullResult, nameof(RequiredRuleCommand.Value), "Value is required.");
        AssertHasError(emptyResult, nameof(RequiredRuleCommand.Value), "Value is required.");
        AssertHasError(whitespaceResult, nameof(RequiredRuleCommand.Value), "Value is required.");
        AssertValid(validResult);
    }

    [Fact]
    public async Task HasText_rejects_null_empty_and_whitespace_text()
    {
        var validator = BuildValidator();

        var nullResult = await validator.ValidateAsync(new HasTextRuleCommand(null));
        var emptyResult = await validator.ValidateAsync(new HasTextRuleCommand(string.Empty));
        var whitespaceResult = await validator.ValidateAsync(new HasTextRuleCommand("   "));
        var validResult = await validator.ValidateAsync(new HasTextRuleCommand("value"));

        AssertHasError(nullResult, nameof(HasTextRuleCommand.Value), "Value must contain text.");
        AssertHasError(emptyResult, nameof(HasTextRuleCommand.Value), "Value must contain text.");
        AssertHasError(whitespaceResult, nameof(HasTextRuleCommand.Value), "Value must contain text.");
        AssertValid(validResult);
    }

    [Fact]
    public async Task NotNull_rejects_null_values()
    {
        var validator = BuildValidator();

        var nullResult = await validator.ValidateAsync(new NotNullRuleCommand(null));
        var validResult = await validator.ValidateAsync(new NotNullRuleCommand("value"));

        AssertHasError(nullResult, nameof(NotNullRuleCommand.Value), "Value must not be null.");
        AssertValid(validResult);
    }

    [Fact]
    public async Task HasItems_rejects_null_and_empty_collections()
    {
        var validator = BuildValidator();

        var nullResult = await validator.ValidateAsync(new HasItemsRuleCommand(null));
        var emptyResult = await validator.ValidateAsync(new HasItemsRuleCommand(Array.Empty<string>()));
        var validResult = await validator.ValidateAsync(new HasItemsRuleCommand(new[] { "value" }));

        AssertHasError(nullResult, nameof(HasItemsRuleCommand.Values), "Values must contain at least one item.");
        AssertHasError(emptyResult, nameof(HasItemsRuleCommand.Values), "Values must contain at least one item.");
        AssertValid(validResult);
    }

    [Fact]
    public async Task Email_rejects_invalid_email_and_allows_empty_values()
    {
        var validator = BuildValidator();

        var invalidResult = await validator.ValidateAsync(new EmailRuleCommand("not-email"));
        var nullResult = await validator.ValidateAsync(new EmailRuleCommand(null));
        var emptyResult = await validator.ValidateAsync(new EmailRuleCommand(string.Empty));
        var validResult = await validator.ValidateAsync(new EmailRuleCommand("person@example.com"));

        AssertHasError(invalidResult, nameof(EmailRuleCommand.Value), "Value must be a valid email address.");
        AssertValid(nullResult);
        AssertValid(emptyResult);
        AssertValid(validResult);
    }

    [Fact]
    public async Task TextLengthAtLeast_rejects_short_text_and_allows_null()
    {
        var validator = BuildValidator();

        var invalidResult = await validator.ValidateAsync(new TextLengthAtLeastRuleCommand("ab"));
        var nullResult = await validator.ValidateAsync(new TextLengthAtLeastRuleCommand(null));
        var boundaryResult = await validator.ValidateAsync(new TextLengthAtLeastRuleCommand("abc"));

        AssertHasError(invalidResult, nameof(TextLengthAtLeastRuleCommand.Value), "Value must contain at least 3 characters.");
        AssertValid(nullResult);
        AssertValid(boundaryResult);
    }

    [Fact]
    public async Task TextLengthAtMost_rejects_long_text_and_allows_null()
    {
        var validator = BuildValidator();

        var invalidResult = await validator.ValidateAsync(new TextLengthAtMostRuleCommand("abcd"));
        var nullResult = await validator.ValidateAsync(new TextLengthAtMostRuleCommand(null));
        var boundaryResult = await validator.ValidateAsync(new TextLengthAtMostRuleCommand("abc"));

        AssertHasError(invalidResult, nameof(TextLengthAtMostRuleCommand.Value), "Value must contain at most 3 characters.");
        AssertValid(nullResult);
        AssertValid(boundaryResult);
    }

    [Fact]
    public async Task Above_rejects_values_equal_to_or_below_threshold()
    {
        var validator = BuildValidator();

        var belowResult = await validator.ValidateAsync(new AboveRuleCommand(9));
        var equalResult = await validator.ValidateAsync(new AboveRuleCommand(10));
        var validResult = await validator.ValidateAsync(new AboveRuleCommand(11));

        AssertHasError(belowResult, nameof(AboveRuleCommand.Value), "Value must be above 10.");
        AssertHasError(equalResult, nameof(AboveRuleCommand.Value), "Value must be above 10.");
        AssertValid(validResult);
    }

    [Fact]
    public async Task AtLeast_rejects_values_below_threshold()
    {
        var validator = BuildValidator();

        var invalidResult = await validator.ValidateAsync(new AtLeastRuleCommand(9));
        var boundaryResult = await validator.ValidateAsync(new AtLeastRuleCommand(10));

        AssertHasError(invalidResult, nameof(AtLeastRuleCommand.Value), "Value must be at least 10.");
        AssertValid(boundaryResult);
    }

    [Fact]
    public async Task Below_rejects_values_equal_to_or_above_threshold()
    {
        var validator = BuildValidator();

        var equalResult = await validator.ValidateAsync(new BelowRuleCommand(10));
        var aboveResult = await validator.ValidateAsync(new BelowRuleCommand(11));
        var validResult = await validator.ValidateAsync(new BelowRuleCommand(9));

        AssertHasError(equalResult, nameof(BelowRuleCommand.Value), "Value must be below 10.");
        AssertHasError(aboveResult, nameof(BelowRuleCommand.Value), "Value must be below 10.");
        AssertValid(validResult);
    }

    [Fact]
    public async Task AtMost_rejects_values_above_threshold()
    {
        var validator = BuildValidator();

        var invalidResult = await validator.ValidateAsync(new AtMostRuleCommand(11));
        var boundaryResult = await validator.ValidateAsync(new AtMostRuleCommand(10));

        AssertHasError(invalidResult, nameof(AtMostRuleCommand.Value), "Value must be at most 10.");
        AssertValid(boundaryResult);
    }

    [Fact]
    public async Task Matches_rejects_pattern_mismatches_and_allows_empty_values()
    {
        var validator = BuildValidator();

        var invalidResult = await validator.ValidateAsync(new MatchesRuleCommand("abc"));
        var nullResult = await validator.ValidateAsync(new MatchesRuleCommand(null));
        var emptyResult = await validator.ValidateAsync(new MatchesRuleCommand(string.Empty));
        var validResult = await validator.ValidateAsync(new MatchesRuleCommand("ABC"));

        AssertHasError(invalidResult, nameof(MatchesRuleCommand.Value), "Value has an invalid format.");
        AssertValid(nullResult);
        AssertValid(emptyResult);
        AssertValid(validResult);
    }

    [Fact]
    public async Task Requires_rejects_when_requirement_returns_false()
    {
        var validator = BuildValidator();

        var invalidResult = await validator.ValidateAsync(new RequiresRuleCommand("BAD-123"));
        var validResult = await validator.ValidateAsync(new RequiresRuleCommand("OK-123"));

        AssertHasError(invalidResult, nameof(RequiresRuleCommand.Value), "Value must start with OK-.");
        AssertValid(validResult);
    }

    [Fact]
    public async Task Use_invokes_custom_async_rule()
    {
        var validator = BuildValidator();

        var invalidResult = await validator.ValidateAsync(new UseRuleCommand("reserved"));
        var validResult = await validator.ValidateAsync(new UseRuleCommand("available"));

        AssertHasError(invalidResult, nameof(UseRuleCommand.Value), "Value is reserved.");
        AssertValid(validResult);
    }

    private static ITinyValidator BuildValidator()
    {
        var services = new ServiceCollection();

        services.UseTinyValidations();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ITinyValidator>();
    }

    private static void AssertValid(ValidationResult result)
    {
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    private static void AssertHasError(ValidationResult result, string member, string message)
    {
        Assert.False(result.IsValid);

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

public sealed record RequiredRuleCommand(string? Value);

public sealed class RequiredRuleCommandValidation : IValidation<RequiredRuleCommand>
{
    public void Define(ValidationRules<RequiredRuleCommand> rules)
    {
        rules.Required(x => x.Value);
    }
}

public sealed record HasTextRuleCommand(string? Value);

public sealed class HasTextRuleCommandValidation : IValidation<HasTextRuleCommand>
{
    public void Define(ValidationRules<HasTextRuleCommand> rules)
    {
        rules.HasText(x => x.Value);
    }
}

public sealed record NotNullRuleCommand(string? Value);

public sealed class NotNullRuleCommandValidation : IValidation<NotNullRuleCommand>
{
    public void Define(ValidationRules<NotNullRuleCommand> rules)
    {
        rules.NotNull(x => x.Value);
    }
}

public sealed record HasItemsRuleCommand(IReadOnlyCollection<string>? Values);

public sealed class HasItemsRuleCommandValidation : IValidation<HasItemsRuleCommand>
{
    public void Define(ValidationRules<HasItemsRuleCommand> rules)
    {
        rules.HasItems(x => x.Values);
    }
}

public sealed record EmailRuleCommand(string? Value);

public sealed class EmailRuleCommandValidation : IValidation<EmailRuleCommand>
{
    public void Define(ValidationRules<EmailRuleCommand> rules)
    {
        rules.Email(x => x.Value);
    }
}

public sealed record TextLengthAtLeastRuleCommand(string? Value);

public sealed class TextLengthAtLeastRuleCommandValidation : IValidation<TextLengthAtLeastRuleCommand>
{
    public void Define(ValidationRules<TextLengthAtLeastRuleCommand> rules)
    {
        rules.TextLengthAtLeast(x => x.Value, 3);
    }
}

public sealed record TextLengthAtMostRuleCommand(string? Value);

public sealed class TextLengthAtMostRuleCommandValidation : IValidation<TextLengthAtMostRuleCommand>
{
    public void Define(ValidationRules<TextLengthAtMostRuleCommand> rules)
    {
        rules.TextLengthAtMost(x => x.Value, 3);
    }
}

public sealed record AboveRuleCommand(int Value);

public sealed class AboveRuleCommandValidation : IValidation<AboveRuleCommand>
{
    public void Define(ValidationRules<AboveRuleCommand> rules)
    {
        rules.Above(x => x.Value, 10);
    }
}

public sealed record AtLeastRuleCommand(int Value);

public sealed class AtLeastRuleCommandValidation : IValidation<AtLeastRuleCommand>
{
    public void Define(ValidationRules<AtLeastRuleCommand> rules)
    {
        rules.AtLeast(x => x.Value, 10);
    }
}

public sealed record BelowRuleCommand(int Value);

public sealed class BelowRuleCommandValidation : IValidation<BelowRuleCommand>
{
    public void Define(ValidationRules<BelowRuleCommand> rules)
    {
        rules.Below(x => x.Value, 10);
    }
}

public sealed record AtMostRuleCommand(int Value);

public sealed class AtMostRuleCommandValidation : IValidation<AtMostRuleCommand>
{
    public void Define(ValidationRules<AtMostRuleCommand> rules)
    {
        rules.AtMost(x => x.Value, 10);
    }
}

public sealed record MatchesRuleCommand(string? Value);

public sealed class MatchesRuleCommandValidation : IValidation<MatchesRuleCommand>
{
    public void Define(ValidationRules<MatchesRuleCommand> rules)
    {
        rules.Matches(x => x.Value, "^[A-Z]{3}$");
    }
}

public sealed record RequiresRuleCommand(string? Value);

public sealed class RequiresRuleCommandValidation : IValidation<RequiresRuleCommand>
{
    public void Define(ValidationRules<RequiresRuleCommand> rules)
    {
        rules.Requires(x => x.Value, RequiresRuleRequirements.StartsWithOk, "Value must start with OK-.");
    }
}

public static class RequiresRuleRequirements
{
    public static bool StartsWithOk(string? value)
    {
        return value is not null && value.StartsWith("OK-", StringComparison.Ordinal);
    }
}

public sealed record UseRuleCommand(string Value);

public sealed class UseRuleCommandValidation : IValidation<UseRuleCommand>
{
    public void Define(ValidationRules<UseRuleCommand> rules)
    {
        rules.Use<UseRuleCustomRule>();
    }
}

public sealed class UseRuleCustomRule : IAsyncValidationRule<UseRuleCommand>
{
    public ValueTask ValidateAsync(
        UseRuleCommand instance,
        ValidationErrorCollection errors,
        CancellationToken cancellationToken)
    {
        if (instance.Value == "reserved")
        {
            errors.Add(nameof(UseRuleCommand.Value), "Value is reserved.");
        }

        return ValueTask.CompletedTask;
    }
}
