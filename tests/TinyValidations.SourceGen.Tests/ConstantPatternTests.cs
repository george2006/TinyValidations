using Microsoft.CodeAnalysis;
using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class ConstantPatternTests
{
    [Theory]
    [InlineData("Patterns.Code", "\"^\\\\d{3}$\"")]
    [InlineData("Pattern", "\"^\\\\d{3}$\"")]
    [InlineData("localPattern", "\"^\\\\d{3}$\"")]
    [InlineData("@\"^\\d\" + \"{3}$\"", "\"^\\\\d{3}$\"")]
    [InlineData("nameof(CreateItem)", "\"CreateItem\"")]
    public void Generates_regex_checks_from_constant_patterns(string argument, string expectedPattern)
    {
        var source = $$"""
using TinyValidations;

public static class Patterns
{
    public const string Code = @"^\d{3}$";
}

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private const string Pattern = @"^\d{3}$";

    public void Define(ValidationRules<CreateItem> rules)
    {
        const string localPattern = @"^\d{3}$";
        rules.Matches(x => x.Code, {{argument}});
    }
}

public sealed record CreateItem(string Code);
""";

        var result = SourceGeneratorTestHost.Run(source);
        var expectedCheck = "Regex.IsMatch(instance.Code, " + expectedPattern + ")";

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();

        var text = result.SingleGeneratedSource();

        Assert.Contains(expectedCheck, text);
    }

    [Theory]
    [InlineData("\"[\"")]
    [InlineData("InvalidPattern")]
    [InlineData("null")]
    [InlineData("NullPattern")]
    [InlineData("RuntimePattern")]
    public void Reports_error_for_invalid_null_or_runtime_patterns(string argument)
    {
        var source = $$"""
using TinyValidations;

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private const string InvalidPattern = "[";
    private const string NullPattern = null;
    private static readonly string RuntimePattern = "^[A-Z]+$";

    public void Define(ValidationRules<CreateItem> rules)
    {
        rules.Matches(x => x.Code, {{argument}});
    }
}

public sealed record CreateItem(string Code);
""";

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = result.SingleDiagnostic();
        var invocation = "rules.Matches(x => x.Code, " + argument + ")";
        var invocationStart = source.IndexOf(invocation, StringComparison.Ordinal);

        result.ShouldHaveNoCompilationErrors();
        result.ShouldGenerateNoSource();
        Assert.Equal("TV0004", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(invocationStart, diagnostic.Location.SourceSpan.Start);
    }
}
