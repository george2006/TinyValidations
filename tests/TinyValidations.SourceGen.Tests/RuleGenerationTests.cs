using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class RuleGenerationTests
{
    [Fact]
    public void Generates_runtime_checks_for_every_allowed_rule()
    {
        var source = """
using TinyValidations;

public sealed class RuleCoverageValidation : IValidation<RuleCoverageCommand>
{
    public void Define(ValidationRules<RuleCoverageCommand> rules)
    {
        rules.Required(x => x.Required);
        rules.HasText(x => x.Text);
        rules.NotNull(x => x.NotNull);
        rules.HasItems(x => x.Items);
        rules.Email(x => x.Email);
        rules.TextLengthAtLeast(x => x.MinimumLength, 3);
        rules.TextLengthAtMost(x => x.MaximumLength, 5);
        rules.Above(x => x.Above, 10);
        rules.AtLeast(x => x.AtLeast, 10);
        rules.Below(x => x.Below, 10);
        rules.AtMost(x => x.AtMost, 10);
        rules.Matches(x => x.Pattern, "^[A-Z]{3}$");
        rules.Requires(x => x.RequiredPrefix, RuleCoverageRequirements.StartsWithOk, "RequiredPrefix must start with OK-.");
        rules.Use<RuleCoverageCustomRule>();
    }
}

public static class RuleCoverageRequirements
{
    public static bool StartsWithOk(string? value) => value is not null && value.StartsWith("OK-");
}

public sealed class RuleCoverageCustomRule : IAsyncValidationRule<RuleCoverageCommand>
{
    public System.Threading.Tasks.ValueTask ValidateAsync(RuleCoverageCommand instance, ValidationErrorCollection errors, System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.ValueTask.CompletedTask;
}

public sealed class RuleCoverageCommand
{
    public string? Required { get; init; }
    public string? Text { get; init; }
    public string? NotNull { get; init; }
    public System.Collections.Generic.IEnumerable<string>? Items { get; init; }
    public string? Email { get; init; }
    public string? MinimumLength { get; init; }
    public string? MaximumLength { get; init; }
    public int Above { get; init; }
    public int AtLeast { get; init; }
    public int Below { get; init; }
    public int AtMost { get; init; }
    public string? Pattern { get; init; }
    public string? RequiredPrefix { get; init; }
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var text = result.SingleGeneratedSource();

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();
        Assert.Contains("if (instance.Required is null)", text);
        Assert.Contains("else if (instance.Required is string __text)", text);
        Assert.Contains("if (string.IsNullOrWhiteSpace(__text))", text);
        Assert.Contains("errors.Add(\"Required\", \"Required is required.\");", text);
        Assert.Contains("if (string.IsNullOrWhiteSpace(instance.Text))", text);
        Assert.Contains("errors.Add(\"Text\", \"Text must contain text.\");", text);
        Assert.Contains("if (instance.NotNull is null)", text);
        Assert.Contains("errors.Add(\"NotNull\", \"NotNull must not be null.\");", text);
        Assert.Contains("if (instance.Items is null)", text);
        Assert.Contains("else if (!instance.Items.Any())", text);
        Assert.Contains("errors.Add(\"Items\", \"Items must contain at least one item.\");", text);
        Assert.Contains("if (!global::TinyValidations.TinyEmailAddress.IsValid(instance.Email))", text);
        Assert.Contains("errors.Add(\"Email\", \"Email must be a valid email address.\");", text);
        Assert.Contains("if (instance.MinimumLength.Length < 3)", text);
        Assert.Contains("errors.Add(\"MinimumLength\", \"MinimumLength must contain at least 3 characters.\");", text);
        Assert.Contains("if (instance.MaximumLength.Length > 5)", text);
        Assert.Contains("errors.Add(\"MaximumLength\", \"MaximumLength must contain at most 5 characters.\");", text);
        Assert.Contains("if (global::System.Collections.Generic.Comparer<int>.Default.Compare(instance.Above, 10) <= 0)", text);
        Assert.Contains("errors.Add(\"Above\", \"Above must be above 10.\");", text);
        Assert.Contains("if (global::System.Collections.Generic.Comparer<int>.Default.Compare(instance.AtLeast, 10) < 0)", text);
        Assert.Contains("errors.Add(\"AtLeast\", \"AtLeast must be at least 10.\");", text);
        Assert.Contains("if (global::System.Collections.Generic.Comparer<int>.Default.Compare(instance.Below, 10) >= 0)", text);
        Assert.Contains("errors.Add(\"Below\", \"Below must be below 10.\");", text);
        Assert.Contains("if (global::System.Collections.Generic.Comparer<int>.Default.Compare(instance.AtMost, 10) > 0)", text);
        Assert.Contains("errors.Add(\"AtMost\", \"AtMost must be at most 10.\");", text);
        Assert.Contains("if (!global::System.Text.RegularExpressions.Regex.IsMatch(instance.Pattern, \"^[A-Z]{3}$\"))", text);
        Assert.Contains("errors.Add(\"Pattern\", \"Pattern has an invalid format.\");", text);
        Assert.Contains("if (!global::RuleCoverageRequirements.StartsWithOk(instance.RequiredPrefix))", text);
        Assert.Contains("errors.Add(\"RequiredPrefix\", \"RequiredPrefix must start with OK-.\");", text);
        Assert.Contains("await _rulecoveragecustomrule.ValidateAsync(instance, errors, cancellationToken).ConfigureAwait(false);", text);
    }

    [Fact]
    public void Comparable_rules_generate_code_for_string_values()
    {
        var source = """
using TinyValidations;

public sealed class SortKeyValidation : IValidation<SortKeyCommand>
{
    public void Define(ValidationRules<SortKeyCommand> rules)
    {
        rules.AtLeast(x => x.Name, "M");
    }
}

public sealed class SortKeyCommand
{
    public string? Name { get; init; }
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var text = result.SingleGeneratedSource();

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();
        Assert.Contains("global::System.Collections.Generic.Comparer<string>.Default.Compare(instance.Name, \"M\") < 0", text);
    }

    [Fact]
    public void Comparable_rules_generate_code_for_nested_value_members()
    {
        var source = """
using TinyValidations;

public sealed class AccountValidation : IValidation<AccountCommand>
{
    public void Define(ValidationRules<AccountCommand> rules)
    {
        rules.AtLeast(x => x.Profile.Age, 18);
    }
}

public sealed class AccountCommand
{
    public AccountProfile? Profile { get; init; }
}

public sealed class AccountProfile
{
    public int Age { get; init; }
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var text = result.SingleGeneratedSource();

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();
        Assert.Contains("global::System.Collections.Generic.Comparer<global::System.Nullable<int>>.Default.Compare(instance.Profile?.Age, 18) < 0", text);
    }

    [Fact]
    public void Selectors_allow_null_forgiving_member_paths()
    {
        var source = """
using TinyValidations;

public sealed class AccountValidation : IValidation<AccountCommand>
{
    public void Define(ValidationRules<AccountCommand> rules)
    {
        rules.Required(x => x.Profile!.Email);
    }
}

public sealed class AccountCommand
{
    public AccountProfile? Profile { get; init; }
}

public sealed class AccountProfile
{
    public string? Email { get; init; }
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var text = result.SingleGeneratedSource();

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();
        Assert.Contains("errors.Add(\"Profile.Email\", \"Profile.Email is required.\");", text);
    }
}
