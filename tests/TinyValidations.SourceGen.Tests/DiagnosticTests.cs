using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class DiagnosticTests
{
    [Fact]
    public void Reports_diagnostic_when_validation_has_no_supported_rules()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0006", diagnostic.Id);
    }

    [Fact]
    public void Reports_diagnostic_when_define_signature_is_wrong()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0001", diagnostic.Id);
        Assert.Equal(
            "Validation declaration 'CreateUserValidation' must contain a Define method with one rules parameter",
            diagnostic.GetMessage());
        Assert.Equal(
            source.IndexOf("CreateUserValidation", StringComparison.Ordinal),
            diagnostic.Location.SourceSpan.Start);
    }

    [Fact]
    public void Reports_diagnostic_when_define_parameter_type_is_wrong()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0001", diagnostic.Id);
    }

    [Fact]
    public void Reports_diagnostic_when_define_return_type_is_wrong()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0001", diagnostic.Id);
    }

    [Fact]
    public void Reports_diagnostic_when_rule_call_is_unsupported()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0002", diagnostic.Id);
    }

    [Fact]
    public void Reports_diagnostic_when_selector_is_unsupported()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0003", diagnostic.Id);
    }

    [Fact]
    public void Reports_diagnostic_when_argument_is_not_literal()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_diagnostic_when_message_is_not_literal()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_diagnostic_when_requires_method_is_not_static()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_diagnostic_when_requires_method_does_not_return_bool()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_diagnostic_when_requires_method_has_wrong_parameter_count()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0004", diagnostic.Id);
    }

    [Fact]
    public void Reports_diagnostic_when_custom_rule_type_is_invalid()
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

        var result = SourceGeneratorTestHost.Run(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0005", diagnostic.Id);
    }
}
