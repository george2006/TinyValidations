using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TinyValidations.SourceGen;
using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class GeneratorSmokeTests
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

        var compilation = CSharpCompilation.Create(
            "Tests",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(TinyValidations.IValidation<>).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValidationSourceGenerator());
        driver = driver.RunGenerators(compilation);

        var result = driver.GetRunResult();
        var generated = Assert.Single(result.GeneratedTrees);
        var text = generated.GetText().ToString();

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

        var compilation = CSharpCompilation.Create(
            "Tests",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(TinyValidations.IValidation<>).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValidationSourceGenerator());
        driver = driver.RunGenerators(compilation);

        var result = driver.GetRunResult();
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

        var result = RunGenerator(source);
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

        var result = RunGenerator(source);
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

        var result = RunGenerator(source);
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

        var result = RunGenerator(source);
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

        var result = RunGenerator(source);
        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("TV0005", diagnostic.Id);
    }

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

        var compilation = CSharpCompilation.Create(
            "Tests",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(TinyValidations.IValidation<>).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValidationSourceGenerator());
        driver = driver.RunGenerators(compilation);

        var result = driver.GetRunResult();

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

        var compilation = CSharpCompilation.Create(
            "Tests",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(TinyValidations.IValidation<>).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValidationSourceGenerator());
        driver = driver.RunGenerators(compilation);

        var result = driver.GetRunResult();
        var generated = Assert.Single(result.GeneratedTrees);
        var text = generated.GetText().ToString();

        Assert.DoesNotContain("Email is required.", text);
    }

    private static GeneratorDriverRunResult RunGenerator(string source)
    {
        var compilation = CSharpCompilation.Create(
            "Tests",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(TinyValidations.IValidation<>).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValidationSourceGenerator());
        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult();
    }
}
