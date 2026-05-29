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

        Assert.Empty(result.Diagnostics);
        Assert.Empty(result.GeneratedTrees);
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
    public string? Email { get; init; }
}
""";

        var result = SourceGeneratorTestHost.Run(source);
        var generated = Assert.Single(result.GeneratedTrees);
        var text = generated.GetText().ToString();

        Assert.DoesNotContain("Email is required.", text);
    }
}
