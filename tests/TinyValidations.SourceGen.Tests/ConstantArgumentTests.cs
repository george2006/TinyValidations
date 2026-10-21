using System.Globalization;
using Microsoft.CodeAnalysis;
using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class ConstantArgumentTests
{
    [Theory]
    [InlineData("TextLengthAtLeast", "Text", "instance.Text.Length < 10", "Text must contain at least 10 characters.")]
    [InlineData("TextLengthAtMost", "Text", "instance.Text.Length > 10", "Text must contain at most 10 characters.")]
    [InlineData("Above", "Value", "Compare(instance.Value, 10) <= 0", "Value must be above 10.")]
    [InlineData("AtLeast", "Value", "Compare(instance.Value, 10) < 0", "Value must be at least 10.")]
    [InlineData("Below", "Value", "Compare(instance.Value, 10) >= 0", "Value must be below 10.")]
    [InlineData("AtMost", "Value", "Compare(instance.Value, 10) > 0", "Value must be at most 10.")]
    public void Generates_constant_values_for_every_length_and_comparison_rule(
        string rule,
        string member,
        string expectedCheck,
        string expectedMessage)
    {
        var source = $$"""
using TinyValidations;

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private const int Threshold = 10;

    public void Define(ValidationRules<CreateItem> rules)
    {
        rules.{{rule}}(x => x.{{member}}, Threshold);
    }
}

public sealed record CreateItem(string Text, int Value);
""";

        var result = SourceGeneratorTestHost.Run(source);

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();

        var text = result.SingleGeneratedSource();

        Assert.Contains(expectedCheck, text);
        Assert.Contains(expectedMessage, text);
        Assert.DoesNotContain("Threshold", text);
    }

    [Theory]
    [InlineData("Limits.MaximumLength")]
    [InlineData("MaximumLength")]
    [InlineData("localMaximumLength")]
    [InlineData("5 + 5")]
    [InlineData("(Limits.MaximumLength)")]
    [InlineData("(int)10L")]
    public void Resolves_constant_fields_local_constants_and_expressions(string argument)
    {
        var source = $$"""
using TinyValidations;

public static class Limits
{
    public const int MaximumLength = 10;
}

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private const int MaximumLength = 10;

    public void Define(ValidationRules<CreateItem> rules)
    {
        const int localMaximumLength = 10;
        rules.TextLengthAtMost(x => x.Text, {{argument}});
    }
}

public sealed record CreateItem(string Text);
""";

        var result = SourceGeneratorTestHost.Run(source);

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();

        var text = result.SingleGeneratedSource();

        Assert.Contains("instance.Text.Length > 10", text);
        Assert.Contains("Text must contain at most 10 characters.", text);
        Assert.DoesNotContain("MaximumLength", text);
    }

    [Theory]
    [InlineData("int", "-10", "-10")]
    [InlineData("long", "long.MinValue", "-9223372036854775808")]
    [InlineData("ulong", "ulong.MaxValue", "18446744073709551615")]
    [InlineData("decimal", "1.234567890123456789m", "1.234567890123456789M")]
    [InlineData("float", "1.23456789f", "1.2345679F")]
    [InlineData("double", "1.2345678901234567d", "1.2345678901234567")]
    [InlineData("float", "float.NaN", "float.NaN")]
    [InlineData("float", "float.PositiveInfinity", "float.PositiveInfinity")]
    [InlineData("float", "float.NegativeInfinity", "float.NegativeInfinity")]
    [InlineData("double", "double.NaN", "double.NaN")]
    [InlineData("double", "double.PositiveInfinity", "double.PositiveInfinity")]
    [InlineData("double", "double.NegativeInfinity", "double.NegativeInfinity")]
    [InlineData("string", "nameof(CreateItem)", "\"CreateItem\"")]
    [InlineData("string", "\"a\\n\\\"b\\\\c\"", "\"a\\n\\\"b\\\\c\"")]
    [InlineData("string", "null", "null")]
    [InlineData("char", "'M'", "'M'")]
    [InlineData("char", "'\\n'", "'\\n'")]
    [InlineData("bool", "true", "true")]
    public void Preserves_constant_values_in_generated_comparisons(string type, string value, string expectedArgument)
    {
        var source = $$"""
using TinyValidations;

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private const {{type}} Threshold = {{value}};

    public void Define(ValidationRules<CreateItem> rules)
    {
        rules.AtLeast(x => x.Value, Threshold);
    }
}

public sealed record CreateItem({{type}} Value);
""";

        var result = SourceGeneratorTestHost.Run(source);
        var expectedCheck = "Compare(instance.Value, " + expectedArgument + ") < 0";

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();

        var text = result.SingleGeneratedSource();

        Assert.Contains(expectedCheck, text);
        Assert.DoesNotContain("Threshold", text);
    }

    [Theory]
    [InlineData("Threshold")]
    [InlineData("\"A\\\"B\\\\C\\nD\"")]
    public void Preserves_text_in_default_messages_for_constant_and_literal_arguments(string argument)
    {
        var source = $$"""
using TinyValidations;

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private const string Threshold = "A\"B\\C\nD";

    public void Define(ValidationRules<CreateItem> rules)
    {
        rules.AtLeast(x => x.Value, {{argument}});
    }
}

public sealed record CreateItem(string Value);
""";

        var result = SourceGeneratorTestHost.Run(source);

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();

        var text = result.SingleGeneratedSource();

        Assert.Contains("errors.Add(\"Value\", \"Value must be at least A\\\"B\\\\C\\nD.\");", text);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("es-ES")]
    public void Generates_decimal_values_and_messages_independently_of_current_culture(string cultureName)
    {
        var source = """
using TinyValidations;

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    public void Define(ValidationRules<CreateItem> rules)
    {
        const decimal minimum = 1.25m;
        rules.AtLeast(x => x.Value, minimum);
    }
}

public sealed record CreateItem(decimal Value);
""";

        var originalCulture = CultureInfo.CurrentCulture;
        var culture = CultureInfo.GetCultureInfo(cultureName);

        try
        {
            CultureInfo.CurrentCulture = culture;

            var result = SourceGeneratorTestHost.Run(source);

            result.ShouldHaveNoDiagnostics();
            result.ShouldHaveNoCompilationErrors();

            var text = result.SingleGeneratedSource();

            Assert.Contains("Compare(instance.Value, 1.25M) < 0", text);
            Assert.Contains("Value must be at least 1.25.", text);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("localThreshold")]
    [InlineData("Threshold")]
    [InlineData("ThresholdProperty")]
    [InlineData("GetThreshold()")]
    public void Rejects_arguments_that_are_not_compile_time_constants(string argument)
    {
        var source = $$"""
using TinyValidations;

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private static readonly int Threshold = 10;
    private static int ThresholdProperty => 10;
    private static int GetThreshold() => 10;

    public void Define(ValidationRules<CreateItem> rules)
    {
        var localThreshold = 10;
        rules.AtLeast(x => x.Value, {{argument}});
    }
}

public sealed record CreateItem(int Value);
""";

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = result.SingleDiagnostic();

        result.ShouldHaveNoCompilationErrors();
        result.ShouldGenerateNoSource();
        Assert.Equal("TV0004", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }
}
