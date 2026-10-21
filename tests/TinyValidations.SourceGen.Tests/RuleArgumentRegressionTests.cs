using Microsoft.CodeAnalysis;
using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class RuleArgumentRegressionTests
{
    [Theory]
    [InlineData("4000")]
    [InlineData("Feedback.MaxTextLength")]
    public void Generates_both_validations_when_text_length_is_a_compile_time_constant(string maximumLength)
    {
        var source = $$"""
using TinyValidations;

public sealed class Feedback
{
    public const int MaxTextLength = 4000;
}

public sealed class GiveFeedbackValidation : IValidation<GiveFeedback>
{
    public void Define(ValidationRules<GiveFeedback> rules)
    {
        rules.TextLengthAtMost(x => x.Text, {{maximumLength}});
    }
}

public sealed class RecordEmotionValidation : IValidation<RecordEmotion>
{
    public void Define(ValidationRules<RecordEmotion> rules)
    {
        rules.HasText(x => x.Name);
    }
}

public sealed record GiveFeedback(string Text);
public sealed record RecordEmotion(string Name);
""";

        var result = SourceGeneratorTestHost.Run(source);

        result.ShouldHaveNoCompilationErrors();
        result.ShouldHaveNoDiagnostics();

        var text = result.SingleGeneratedSource();

        Assert.Contains("ITinyValidationRunner<global::GiveFeedback>", text);
        Assert.Contains("ITinyValidationRunner<global::RecordEmotion>", text);
        Assert.Contains("Text must contain at most 4000 characters.", text);
    }

    [Fact]
    public void Reports_error_when_rule_argument_is_not_a_compile_time_constant()
    {
        var result = SourceGeneratorTestHost.Run(UnsupportedArgumentSource);
        var diagnostic = result.SingleDiagnostic();
        var invocation = "rules.TextLengthAtMost(x => x.Text, Feedback.MaxTextLength)";
        var invocationStart = UnsupportedArgumentSource.IndexOf(invocation, StringComparison.Ordinal);

        result.ShouldHaveNoCompilationErrors();
        Assert.Equal("TV0004", diagnostic.Id);
        Assert.Equal(invocationStart, diagnostic.Location.SourceSpan.Start);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [Fact]
    public void Generates_valid_validation_when_another_validation_has_an_unsupported_argument()
    {
        var result = SourceGeneratorTestHost.Run(UnsupportedArgumentSource);
        var diagnostic = result.SingleDiagnostic();

        result.ShouldHaveNoCompilationErrors();
        Assert.Equal("TV0004", diagnostic.Id);

        var text = result.SingleGeneratedSource();

        Assert.Contains("ITinyValidationRunner<global::RecordEmotion>", text);
        Assert.DoesNotContain("ITinyValidationRunner<global::GiveFeedback>", text);
        Assert.DoesNotContain("typeof(global::GiveFeedbackValidation)", text);
    }

    private const string UnsupportedArgumentSource = """
using TinyValidations;

public sealed class Feedback
{
    public static readonly int MaxTextLength = 4000;
}

public sealed class GiveFeedbackValidation : IValidation<GiveFeedback>
{
    public void Define(ValidationRules<GiveFeedback> rules)
    {
        rules.HasText(x => x.Text);
        rules.TextLengthAtMost(x => x.Text, Feedback.MaxTextLength);
    }
}

public sealed class RecordEmotionValidation : IValidation<RecordEmotion>
{
    public void Define(ValidationRules<RecordEmotion> rules)
    {
        rules.HasText(x => x.Name);
    }
}

public sealed record GiveFeedback(string Text);
public sealed record RecordEmotion(string Name);
""";
}
