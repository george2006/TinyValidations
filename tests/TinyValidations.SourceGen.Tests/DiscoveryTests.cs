using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class DiscoveryTests
{
    [Fact]
    public void Ignores_unrelated_i_validation_interfaces()
    {
        var source = """
namespace Other;

public interface IValidation<T>
{
}

public sealed class CreateUserValidation : IValidation<CreateUser>
{
}

public sealed class CreateUser
{
}
""";

        var result = SourceGeneratorTestHost.Run(source);

        result.ShouldHaveNoDiagnostics();
        result.ShouldGenerateNoSource();
    }

    [Fact]
    public void Ignores_methods_that_only_look_like_validation_rules()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Required(x => x.Name);

        var other = new OtherRules();
        other.Required(x => x.Email);
    }
}

public sealed class OtherRules
{
    public void Required(System.Func<CreateUser, string?> member)
    {
    }
}

public sealed class CreateUser
{
    public string? Name { get; init; }
    public string? Email { get; init; }
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var text = result.SingleGeneratedSource();

        result.ShouldHaveNoDiagnostics();
        Assert.Contains("Name is required.", text);
        Assert.DoesNotContain("Email is required.", text);
    }

    [Fact]
    public void Ignores_validation_rule_calls_outside_define_method()
    {
        var source = """
using TinyValidations;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Required(x => x.Email);
    }

    public void Configure(ValidationRules<CreateUser> rules)
    {
        rules.Required(x => x.DisplayName);
    }
}

public sealed class CreateUser
{
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var text = result.SingleGeneratedSource();

        result.ShouldHaveNoDiagnostics();
        Assert.Contains("Email is required.", text);
        Assert.DoesNotContain("DisplayName is required.", text);
    }
}
