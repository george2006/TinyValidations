using Microsoft.CodeAnalysis;
using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class NamedArgumentTests
{
    public static IEnumerable<object[]> RuleCalls()
    {
        var rules = new[]
        {
            new RuleCase
            {
                Name = "Required",
                Type = "string",
                ValidValue = "\"ABC\"",
                InvalidValue = "null"
            },
            new RuleCase
            {
                Name = "HasText",
                Type = "string",
                ValidValue = "\"ABC\"",
                InvalidValue = "\" \""
            },
            new RuleCase
            {
                Name = "NotNull",
                Type = "string",
                ValidValue = "\"ABC\"",
                InvalidValue = "null"
            },
            new RuleCase
            {
                Name = "HasItems",
                Type = "string[]",
                ValidValue = "new[] { \"item\" }",
                InvalidValue = "Array.Empty<string>()"
            },
            new RuleCase
            {
                Name = "Email",
                Type = "string",
                ValidValue = "\"a@example.com\"",
                InvalidValue = "\"invalid\""
            },
            new RuleCase
            {
                Name = "TextLengthAtLeast",
                Type = "string",
                ValueParameter = "length",
                ValueArgument = "Length",
                ValidValue = "\"ABC\"",
                InvalidValue = "\"AB\""
            },
            new RuleCase
            {
                Name = "TextLengthAtMost",
                Type = "string",
                ValueParameter = "length",
                ValueArgument = "Length",
                ValidValue = "\"ABC\"",
                InvalidValue = "\"ABCD\""
            },
            new RuleCase
            {
                Name = "Above",
                Type = "int",
                ValueParameter = "value",
                ValueArgument = "Threshold",
                ValidValue = "11",
                InvalidValue = "10"
            },
            new RuleCase
            {
                Name = "AtLeast",
                Type = "int",
                ValueParameter = "value",
                ValueArgument = "Threshold",
                ValidValue = "10",
                InvalidValue = "9"
            },
            new RuleCase
            {
                Name = "Below",
                Type = "int",
                ValueParameter = "value",
                ValueArgument = "Threshold",
                ValidValue = "9",
                InvalidValue = "10"
            },
            new RuleCase
            {
                Name = "AtMost",
                Type = "int",
                ValueParameter = "value",
                ValueArgument = "Threshold",
                ValidValue = "10",
                InvalidValue = "11"
            },
            new RuleCase
            {
                Name = "Matches",
                Type = "string",
                ValueParameter = "pattern",
                ValueArgument = "Pattern",
                ValidValue = "\"ABC\"",
                InvalidValue = "\"invalid\""
            },
            new RuleCase
            {
                Name = "Requires",
                Type = "string",
                ValueParameter = "requirement",
                ValueArgument = "Requirements.IsValid",
                ValidValue = "\"ABC\"",
                InvalidValue = "\"invalid\""
            }
        };

        foreach (var rule in rules)
        {
            foreach (var form in new[] { "positional", "named", "reordered", "mixed", "parenthesized", "default-message" })
            {
                if (rule.Name == "Requires" && form == "default-message")
                {
                    continue;
                }

                yield return new object[]
                {
                    rule.Name,
                    rule.Type,
                    rule.ValueParameter,
                    rule.ValueArgument,
                    rule.ValidValue,
                    rule.InvalidValue,
                    form
                };
            }
        }
    }

    [Theory]
    [MemberData(nameof(RuleCalls))]
    public async Task Named_arguments_preserve_generated_checks_and_runtime_results(
        string rule,
        string type,
        string valueParameter,
        string valueArgument,
        string validValue,
        string invalidValue,
        string form)
    {
        var includeMessage = form != "default-message";
        var positionalArguments = GetPositionalArguments(valueArgument, includeMessage);
        var namedArguments = GetArguments(valueParameter, valueArgument, form);
        var positionalSource = CreateSource(rule, type, positionalArguments, validValue, invalidValue);
        var namedSource = CreateSource(rule, type, namedArguments, validValue, invalidValue);

        var positional = SourceGeneratorTestHost.Run(positionalSource);
        var named = SourceGeneratorTestHost.Run(namedSource);

        positional.ShouldHaveNoDiagnostics();
        positional.ShouldHaveNoCompilationErrors();
        named.ShouldHaveNoDiagnostics();
        named.ShouldHaveNoCompilationErrors();

        var expectedSource = positional.SingleGeneratedSource();
        var actualSource = named.SingleGeneratedSource();

        Assert.Equal(expectedSource, actualSource);

        var assembly = named.LoadAssembly();
        var validation = assembly.GetType("CreateItemValidation")!;
        var validate = validation.GetMethod("Validate")!.CreateDelegate<Func<bool, Task<ValidationResult>>>();
        var valid = await validate(true);
        var invalid = await validate(false);

        Assert.True(valid.IsValid);
        Assert.Empty(valid.Errors);
        Assert.False(invalid.IsValid);

        var error = Assert.Single(invalid.Errors);

        Assert.Equal("Value", error.Member);
        if (includeMessage)
        {
            Assert.Equal("invalid", error.Message);
        }
    }

    [Theory]
    [InlineData("Matches(x => x.Value, message: Message, pattern: RuntimeValue)", "TV0004")]
    [InlineData("Matches(message: Message, pattern: \"[\", member: x => x.Value)", "TV0004")]
    [InlineData("Required(message: RuntimeValue, member: x => x.Value)", "TV0004")]
    [InlineData("Required(message: Message, member: x => x.Value.Trim())", "TV0003")]
    [InlineData("Requires(message: Message, requirement: requirements.IsValid, member: x => x.Value)", "TV0004")]
    public void Reports_invalid_named_arguments_and_preserves_unrelated_validation(string call, string diagnosticId)
    {
        var source = $$"""
using TinyValidations;

public sealed record CreateItem(string Value);
public sealed record OtherItem(string Value);

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private const string Message = "invalid";
    private static readonly string RuntimeValue = "invalid";

    public void Define(ValidationRules<CreateItem> rules)
    {
        var requirements = new Requirements();
        rules.{{call}};
    }
}

public sealed class Requirements
{
    public bool IsValid(string value) => value == "ABC";
}

public sealed class OtherItemValidation : IValidation<OtherItem>
{
    public void Define(ValidationRules<OtherItem> rules) => rules.HasText(x => x.Value);
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = result.SingleDiagnostic();

        result.ShouldHaveNoCompilationErrors();
        Assert.Equal(diagnosticId, diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);

        var text = result.SingleGeneratedSource();

        Assert.Contains("ITinyValidationRunner<global::OtherItem>", text);
        Assert.DoesNotContain("CreateItem", text);
    }

    [Theory]
    [InlineData("Required(member: x => x.Value, unknown: \"invalid\")")]
    [InlineData("Required(member: x => x.Value, member: x => x.Value)")]
    [InlineData("Matches(member: x => x.Value, message: \"invalid\")")]
    [InlineData("Requires(member: x => x.Value, requirement: Requirements.IsValid, unknown: \"invalid\")")]
    public void Reports_error_when_named_arguments_cannot_be_bound(string call)
    {
        var source = $$"""
using TinyValidations;

public sealed record CreateItem(string Value);

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    public void Define(ValidationRules<CreateItem> rules)
    {
        rules.{{call}};
    }
}

public static class Requirements
{
    public static bool IsValid(string value) => value == "ABC";
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = result.SingleDiagnostic();

        result.ShouldGenerateNoSource();
        Assert.Equal("TV0004", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    private static string GetPositionalArguments(string valueArgument, bool includeMessage)
    {
        var arguments = "x => x.Value";
        if (valueArgument.Length > 0)
        {
            arguments += ", " + valueArgument;
        }

        if (includeMessage)
        {
            arguments += ", Message";
        }

        return arguments;
    }

    private static string GetArguments(string valueParameter, string valueArgument, string form)
    {
        var valueExpression = valueArgument;
        if (form == "parenthesized" && valueParameter != "requirement")
        {
            valueExpression = "((" + valueArgument + "))";
        }

        var value = valueParameter.Length > 0 ? valueParameter + ": " + valueExpression + ", " : string.Empty;

        switch (form)
        {
            case "positional": return GetPositionalArguments(valueArgument, true);
            case "named": return "member: x => x.Value, " + value + "message: Message";
            case "reordered": return "message: Message, " + value + "member: x => x.Value";
            case "parenthesized": return "message: ((Message)), " + value + "member: x => x.Value";
            case "mixed":
                return valueParameter.Length > 0
                    ? "x => x.Value, message: Message, " + valueParameter + ": " + valueArgument
                    : "x => x.Value, message: Message";
            case "default-message": return value + "member: x => x.Value";
            default: throw new ArgumentOutOfRangeException(nameof(form));
        }
    }

    private static string CreateSource(string rule, string type, string arguments, string validValue, string invalidValue)
    {
        return $$"""
using System;
using System.Threading.Tasks;
using TinyValidations;

public sealed record CreateItem({{type}} Value);

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private const string Message = "invalid";
    private const string Pattern = "^[A-Z]+$";
    private const int Length = 3;
    private const int Threshold = 10;

    public void Define(ValidationRules<CreateItem> rules)
    {
        rules.{{rule}}({{arguments}});
    }

    public static async Task<ValidationResult> Validate(bool valid)
    {
        var value = valid ? {{validValue}} : {{invalidValue}};
        var command = new CreateItem(value);
        var runner = new TinyValidations.Generated.CreateItemValidationRunner();
        return await runner.ValidateAsync(command, default);
    }
}

public static class Requirements
{
    public static bool IsValid(string value) => value == "ABC";
}
""";
    }

    private sealed class RuleCase
    {
        public required string Name { get; init; }
        public required string Type { get; init; }
        public string ValueParameter { get; init; } = string.Empty;
        public string ValueArgument { get; init; } = string.Empty;
        public required string ValidValue { get; init; }
        public required string InvalidValue { get; init; }
    }
}
