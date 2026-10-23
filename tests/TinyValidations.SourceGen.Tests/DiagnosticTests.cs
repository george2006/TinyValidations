using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class DiagnosticTests
{
    [Fact]
    public void Reports_error_when_validation_has_no_supported_rules()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
    }
}

public sealed class CreateUser
{
    public string? Email { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0006", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_define_signature_is_wrong()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define()
    {
    }
}

public sealed class CreateUser
{
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0001", diagnostic.Id);
        Assert.Equal(
            "Validation declaration 'CreateUserValidation' must contain a Define method with one rules parameter",
            diagnostic.GetMessage());
        Assert.Equal(
            source.IndexOf("CreateUserValidation", StringComparison.Ordinal),
            diagnostic.Location.SourceSpan.Start);
    }

    [Fact]
    public void Reports_error_when_define_parameter_type_is_wrong()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(string rules)
    {
    }
}

public sealed class CreateUser
{
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0001", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_define_return_type_is_wrong()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public int Define(ValidationRules<CreateUser> rules)
    {
        return 0;
    }
}

public sealed class CreateUser
{
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0001", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_rule_call_is_unsupported()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Unknown(x => x.Email);
    }
}

public sealed class CreateUser
{
    public string? Email { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0002", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_selector_is_unsupported()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Required(x => x.Email!.ToString());
    }
}

public sealed class CreateUser
{
    public string? Email { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0003", diagnostic.Id);
    }

    [Theory]
    [InlineData(
        "Matches(x => x.Code.Trim(), RuntimePattern, RuntimeMessage)",
        "x => x.Code.Trim()")]
    [InlineData(
        "Matches(message: RuntimeMessage, pattern: RuntimePattern, member: x => x.Code.Trim())",
        "member: x => x.Code.Trim()")]
    public void Reports_selector_error_before_invalid_value_and_message_arguments(string ruleCall, string selector)
    {
        var source = $$"""
using TinyValidations;

public sealed record CreateItem(string Code);

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private static readonly string RuntimePattern = "^[A-Z]+$";
    private static readonly string RuntimeMessage = "Invalid code.";

    public void Define(ValidationRules<CreateItem> rules)
    {
        rules.{{ruleCall}};
    }
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = result.SingleDiagnostic();
        var selectorStart = source.IndexOf(selector, StringComparison.Ordinal);

        result.ShouldHaveNoCompilationErrors();
        result.ShouldGenerateNoSource();
        Assert.Equal("TV0003", diagnostic.Id);
        Assert.Equal(Microsoft.CodeAnalysis.DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(selectorStart, diagnostic.Location.SourceSpan.Start);
        Assert.Equal(selector.Length, diagnostic.Location.SourceSpan.Length);
    }

    [Fact]
    public void Reports_error_when_argument_is_not_literal()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        var length = 2;
        rules.TextLengthAtLeast(x => x.Email, length);
    }
}

public sealed class CreateUser
{
    public string? Email { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_rule_value_argument_is_missing()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.TextLengthAtLeast(x => x.Email);
    }
}

public sealed class CreateUser
{
    public string? Email { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_rule_has_too_many_arguments()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Required(x => x.Email, "Email is required.", "extra");
    }
}

public sealed class CreateUser
{
    public string? Email { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_message_is_not_a_compile_time_constant()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        var message = "Email is required.";
        rules.Required(x => x.Email, message);
    }
}

public sealed class CreateUser
{
    public string? Email { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_matches_pattern_is_invalid()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Matches(x => x.Code, "[");
    }
}

public sealed class CreateUser
{
    public string? Code { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Theory]
    [InlineData(
        "Requires(x => x.Code.Trim(), requirements.IsValid, RuntimeMessage)",
        "TV0003", "x => x.Code.Trim()")]
    [InlineData(
        "Requires(message: RuntimeMessage, requirement: requirements.IsValid, member: x => x.Code.Trim())",
        "TV0003", "member: x => x.Code.Trim()")]
    [InlineData(
        "Requires(x => x.Code, requirements.IsValid, RuntimeMessage)",
        "TV0004", "requirements.IsValid")]
    [InlineData(
        "Requires(message: RuntimeMessage, requirement: requirements.IsValid, member: x => x.Code)",
        "TV0004", "requirement: requirements.IsValid")]
    [InlineData(
        "Requires(x => x.Code, Requirements.Accepts, RuntimeMessage)",
        "TV0004", "RuntimeMessage")]
    [InlineData(
        "Requires(message: RuntimeMessage, requirement: Requirements.Accepts, member: x => x.Code)",
        "TV0004", "message: RuntimeMessage")]
    public void Reports_requires_errors_in_validation_order_at_the_failing_argument(
        string ruleCall,
        string diagnosticId,
        string argument)
    {
        var source = $$"""
using TinyValidations;

public sealed record CreateItem(string Code);

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    private static readonly string RuntimeMessage = "Invalid code.";

    public void Define(ValidationRules<CreateItem> rules)
    {
        var requirements = new Requirements();
        rules.{{ruleCall}};
    }
}

public sealed class Requirements
{
    public bool IsValid(string value) => value.Length > 0;
    public static bool Accepts(string value) => value.Length > 0;
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = result.SingleDiagnostic();
        var invocationStart = source.IndexOf(ruleCall, StringComparison.Ordinal);
        var argumentStart = source.IndexOf(argument, invocationStart, StringComparison.Ordinal);

        result.ShouldHaveNoCompilationErrors();
        result.ShouldGenerateNoSource();
        Assert.Equal(diagnosticId, diagnostic.Id);
        Assert.Equal(Microsoft.CodeAnalysis.DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(argumentStart, diagnostic.Location.SourceSpan.Start);
        Assert.Equal(argument.Length, diagnostic.Location.SourceSpan.Length);
    }

    [Fact]
    public void Reports_error_when_requires_method_is_not_static()
    {
        var source = """
using TinyValidations;

public sealed class CreateOrderValidation : IValidation<CreateOrder>
{
    public void Define(ValidationRules<CreateOrder> rules)
    {
        var requirements = new OrderNumberRequirements();
        rules.Requires(x => x.OrderNumber, requirements.HasOrderPrefix, "Order number must start with ORD-.");
    }
}

public sealed class OrderNumberRequirements
{
    public bool HasOrderPrefix(string? value)
    {
        return value is not null && value.StartsWith("ORD-");
    }
}

public sealed class CreateOrder
{
    public string? OrderNumber { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_requires_method_does_not_return_bool()
    {
        var source = """
using TinyValidations;

public sealed class CreateOrderValidation : IValidation<CreateOrder>
{
    public void Define(ValidationRules<CreateOrder> rules)
    {
        rules.Requires(x => x.OrderNumber, OrderNumberRequirements.HasOrderPrefix, "Order number must start with ORD-.");
    }
}

public static class OrderNumberRequirements
{
    public static string HasOrderPrefix(string? value)
    {
        return "";
    }
}

public sealed class CreateOrder
{
    public string? OrderNumber { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_requires_method_has_wrong_parameter_count()
    {
        var source = """
using TinyValidations;

public sealed class CreateOrderValidation : IValidation<CreateOrder>
{
    public void Define(ValidationRules<CreateOrder> rules)
    {
        rules.Requires(x => x.OrderNumber, OrderNumberRequirements.HasOrderPrefix, "Order number must start with ORD-.");
    }
}

public static class OrderNumberRequirements
{
    public static bool HasOrderPrefix(string? value, string prefix)
    {
        return value is not null && value.StartsWith(prefix);
    }
}

public sealed class CreateOrder
{
    public string? OrderNumber { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_requires_rule_has_too_many_arguments()
    {
        var source = """
using TinyValidations;

public sealed class CreateOrderValidation : IValidation<CreateOrder>
{
    public void Define(ValidationRules<CreateOrder> rules)
    {
        rules.Requires(x => x.OrderNumber, OrderNumberRequirements.HasOrderPrefix, "Order number must start with ORD-.", "extra");
    }
}

public static class OrderNumberRequirements
{
    public static bool HasOrderPrefix(string? value)
    {
        return value is not null && value.StartsWith("ORD-");
    }
}

public sealed class CreateOrder
{
    public string? OrderNumber { get; init; }
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_error_when_custom_rule_type_is_invalid()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Use<NotACustomRule>();
    }
}

public sealed class NotACustomRule
{
}

public sealed class CreateUser
{
}
""";

        var diagnostic = SingleDiagnosticWithNoSource(source);

        Assert.Equal("TV0005", diagnostic.Id);
    }

    private static Microsoft.CodeAnalysis.Diagnostic SingleDiagnosticWithNoSource(string source)
    {
        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = result.SingleDiagnostic();

        result.ShouldGenerateNoSource();
        Assert.Equal(Microsoft.CodeAnalysis.DiagnosticSeverity.Error, diagnostic.Severity);

        return diagnostic;
    }
}

