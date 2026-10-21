using Microsoft.CodeAnalysis;
using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class ConstantMessageTests
{
    [Theory]
    [InlineData("Required(x => x.Text, Message)", "Text")]
    [InlineData("HasText(x => x.Text, Message)", "Text")]
    [InlineData("NotNull(x => x.Text, Message)", "Text")]
    [InlineData("HasItems(x => x.Items, Message)", "Items")]
    [InlineData("Email(x => x.Text, Message)", "Text")]
    [InlineData("TextLengthAtLeast(x => x.Text, 3, Message)", "Text")]
    [InlineData("TextLengthAtMost(x => x.Text, 10, Message)", "Text")]
    [InlineData("Above(x => x.Value, 10, Message)", "Value")]
    [InlineData("AtLeast(x => x.Value, 10, Message)", "Value")]
    [InlineData("Below(x => x.Value, 10, Message)", "Value")]
    [InlineData("AtMost(x => x.Value, 10, Message)", "Value")]
    [InlineData("Matches(x => x.Text, \"^[A-Z]+$\", Message)", "Text")]
    [InlineData("Requires(x => x.Text, Requirements.IsValid, Message)", "Text")]
    public void Generates_constant_messages_for_every_rule_that_accepts_a_message(string ruleCall, string member)
    {
        var source = $$"""
using TinyValidations;

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private const string Message = "Check \"value\".\nTry again.";

    public void Define(ValidationRules<CreateItem> rules)
    {
        rules.{{ruleCall}};
    }
}

public static class Requirements
{
    public static bool IsValid(string value) => value == "OK";
}

public sealed record CreateItem(string Text, int Value, string[] Items);
""";

        var result = SourceGeneratorTestHost.Run(source);
        var expectedError = "errors.Add(\"" + member + "\", \"Check \\\"value\\\".\\nTry again.\");";

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();

        var text = result.SingleGeneratedSource();

        Assert.Contains(expectedError, text);
        Assert.DoesNotContain(", Message)", text);
    }

    [Theory]
    [InlineData("Required(x => x.Text, Messages.Invalid)")]
    [InlineData("Required(x => x.Text, localMessage)")]
    [InlineData("Required(x => x.Text, nameof(CreateItem.Text))")]
    [InlineData("Required(x => x.Text, \"Te\" + \"xt\")")]
    [InlineData("Requires(x => x.Text, Requirements.IsValid, Messages.Invalid)")]
    [InlineData("Requires(x => x.Text, Requirements.IsValid, localMessage)")]
    [InlineData("Requires(x => x.Text, Requirements.IsValid, nameof(CreateItem.Text))")]
    [InlineData("Requires(x => x.Text, Requirements.IsValid, \"Te\" + \"xt\")")]
    public void Resolves_message_constant_forms(string ruleCall)
    {
        var source = $$"""
using TinyValidations;

public static class Messages
{
    public const string Invalid = "Text";
}

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    public void Define(ValidationRules<CreateItem> rules)
    {
        const string localMessage = "Text";
        rules.{{ruleCall}};
    }
}

public static class Requirements
{
    public static bool IsValid(string value) => value == "OK";
}

public sealed record CreateItem(string Text);
""";

        var result = SourceGeneratorTestHost.Run(source);

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();

        var text = result.SingleGeneratedSource();

        Assert.Contains("errors.Add(\"Text\", \"Text\");", text);
    }

    [Theory]
    [InlineData("Message")]
    [InlineData("localMessage")]
    [InlineData("MessageProperty")]
    [InlineData("GetMessage()")]
    public void Rejects_runtime_messages_in_member_and_requirement_rules(string argument)
    {
        var source = $$"""
using TinyValidations;

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private static readonly string Message = "Invalid text.";
    private static string MessageProperty => "Invalid text.";
    private static string GetMessage() => "Invalid text.";

    public void Define(ValidationRules<CreateItem> rules)
    {
        var localMessage = "Invalid text.";
        rules.Required(x => x.Text, {{argument}});
        rules.Requires(x => x.Text, Requirements.IsValid, {{argument}});
    }
}

public static class Requirements
{
    public static bool IsValid(string value) => value == "OK";
}

public sealed record CreateItem(string Text);
""";

        var result = SourceGeneratorTestHost.Run(source);

        result.ShouldHaveNoCompilationErrors();
        result.ShouldGenerateNoSource();
        Assert.Equal(2, result.Diagnostics.Count);
        Assert.All(result.Diagnostics, diagnostic =>
        {
            Assert.Equal("TV0004", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        });
    }
}
