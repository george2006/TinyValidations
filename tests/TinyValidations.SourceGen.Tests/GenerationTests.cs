using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class GenerationTests
{
    [Fact]
    public void Generates_runner_contribution_and_module_initializer()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Required(x => x.Email);
        rules.Email(x => x.Email);
        rules.AtLeast(x => x.Age, 18);
        rules.Use<UniqueEmailRule>();
    }
}

public sealed class UniqueEmailRule : IAsyncValidationRule<CreateUser>
{
    public System.Threading.Tasks.ValueTask ValidateAsync(CreateUser instance, ValidationErrorCollection errors, System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.ValueTask.CompletedTask;
}

public sealed class CreateUser
{
    public string? Email { get; init; }
    public int Age { get; init; }
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var text = result.SingleGeneratedSource();

        Assert.Contains("ITinyValidationRunner<global::CreateUser>", text);
        Assert.Contains("TinyGeneratedValidationContribution", text);
        Assert.Contains("TinyGeneratedValidationModuleInitializer", text);
        Assert.Contains("TinyValidationBootstrap.AddContribution", text);
        Assert.Contains("TinyValidationContributionAttribute(typeof(global::CreateUser)", text);
        Assert.Contains("UniqueEmailRule", text);
        Assert.Contains("await _uniqueemailrule.ValidateAsync", text);
        Assert.Contains("ServiceDescriptor.Scoped", text);
        Assert.Contains("TryAddEnumerable", text);
        Assert.Contains("TryAddScoped<global::UniqueEmailRule>", text);
        Assert.DoesNotContain("System.Reflection", text);
        Assert.DoesNotContain("GetType()", text);
        Assert.DoesNotContain("typeof(T)", text);
    }

    [Fact]
    public void Generates_rules_from_explicit_define_implementation()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    void IValidation<CreateUser>.Define(ValidationRules<CreateUser> rules)
    {
        rules.Required(x => x.Email);
    }
}

public sealed class CreateUser
{
    public string? Email { get; init; }
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var text = result.SingleGeneratedSource();

        result.ShouldHaveNoDiagnostics();
        Assert.Contains("ITinyValidationRunner<global::CreateUser>", text);
        Assert.Contains("Email is required.", text);
    }

    [Fact]
    public void Generates_static_requires_rule_call()
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

        var result = SourceGeneratorTestHost.Run(source);
        var text = result.SingleGeneratedSource();

        result.ShouldHaveNoDiagnostics();
        Assert.Contains("if (!global::OrderNumberRequirements.HasOrderPrefix(instance.OrderNumber))", text);
        Assert.Contains("errors.Add(\"OrderNumber\", \"Order number must start with ORD-.\");", text);
    }
}
