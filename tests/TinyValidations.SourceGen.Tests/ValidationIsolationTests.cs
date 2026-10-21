using Microsoft.CodeAnalysis;
using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class ValidationIsolationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Generates_valid_validation_and_excludes_invalid_validation_regardless_of_order(bool invalidValidationFirst)
    {
        var invalidValidation = """
public sealed class GiveFeedbackValidation : IValidation<GiveFeedback>
{
    private static readonly int MaximumLength = 4000;

    public void Define(ValidationRules<GiveFeedback> rules)
    {
        rules.HasText(x => x.Text);
        rules.TextLengthAtMost(x => x.Text, MaximumLength);
    }
}

public sealed record GiveFeedback(string Text);
""";

        var declarations = invalidValidationFirst
            ? invalidValidation + "\n" + ValidValidation
            : ValidValidation + "\n" + invalidValidation;
        var source = "using TinyValidations;\n" + declarations;

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = result.SingleDiagnostic();
        var invocation = "rules.TextLengthAtMost(x => x.Text, MaximumLength)";
        var invocationStart = source.IndexOf(invocation, StringComparison.Ordinal);

        Assert.Equal("TV0004", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(invocationStart, diagnostic.Location.SourceSpan.Start);

        var text = result.SingleGeneratedSource();

        result.ShouldHaveNoCompilationErrors();
        Assert.Contains("ITinyValidationRunner<global::RecordEmotion>", text);
        Assert.Contains("TinyValidationContributionAttribute(typeof(global::RecordEmotion)", text);
        Assert.DoesNotContain("GiveFeedback", text);
    }

    [Fact]
    public void Generates_valid_validation_when_another_validation_is_empty()
    {
        var source = "using TinyValidations;\n" + EmptyValidation + "\n" + ValidValidation;

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = result.SingleDiagnostic();

        Assert.Equal("TV0006", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);

        var text = result.SingleGeneratedSource();

        result.ShouldHaveNoCompilationErrors();
        Assert.Contains("ITinyValidationRunner<global::RecordEmotion>", text);
        Assert.DoesNotContain("DeleteFeedback", text);
    }

    [Fact]
    public void Reports_all_errors_and_generates_valid_validation()
    {
        var source = "using TinyValidations;\n" + InvalidRulesValidation + "\n" + EmptyValidation + "\n" + ValidValidation;

        var result = SourceGeneratorTestHost.Run(source);
        var diagnosticIds = result.Diagnostics
            .Select(diagnostic => diagnostic.Id)
            .OrderBy(id => id)
            .ToArray();
        var expectedDiagnosticIds = new[] { "TV0003", "TV0004", "TV0006" };

        Assert.Equal(expectedDiagnosticIds, diagnosticIds);
        Assert.All(result.Diagnostics, diagnostic => Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity));

        var text = result.SingleGeneratedSource();

        result.ShouldHaveNoCompilationErrors();
        Assert.Contains("ITinyValidationRunner<global::RecordEmotion>", text);
        Assert.DoesNotContain("GiveFeedback", text);
        Assert.DoesNotContain("DeleteFeedback", text);
    }

    [Fact]
    public void Reports_all_errors_and_generates_no_source_when_all_validations_are_invalid()
    {
        var source = "using TinyValidations;\n" + InvalidRulesValidation + "\n" + EmptyValidation;

        var result = SourceGeneratorTestHost.Run(source);
        var diagnosticIds = result.Diagnostics
            .Select(diagnostic => diagnostic.Id)
            .OrderBy(id => id)
            .ToArray();
        var expectedDiagnosticIds = new[] { "TV0003", "TV0004", "TV0006" };

        result.ShouldHaveNoCompilationErrors();
        result.ShouldGenerateNoSource();
        Assert.Equal(expectedDiagnosticIds, diagnosticIds);
        Assert.All(result.Diagnostics, diagnostic => Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity));
    }

    private const string ValidValidation = """
public sealed class RecordEmotionValidation : IValidation<RecordEmotion>
{
    public void Define(ValidationRules<RecordEmotion> rules)
    {
        rules.HasText(x => x.Name);
    }
}

public sealed record RecordEmotion(string Name);
""";

    private const string EmptyValidation = """
public sealed class DeleteFeedbackValidation : IValidation<DeleteFeedback>
{
    public void Define(ValidationRules<DeleteFeedback> rules)
    {
    }
}

public sealed record DeleteFeedback(int Id);
""";

    private const string InvalidRulesValidation = """
public sealed class GiveFeedbackValidation : IValidation<GiveFeedback>
{
    private static readonly int MaximumLength = 4000;

    public void Define(ValidationRules<GiveFeedback> rules)
    {
        rules.Required(x => x.Text.Trim());
        rules.TextLengthAtMost(x => x.Text, MaximumLength);
    }
}

public sealed record GiveFeedback(string Text);
""";
}
