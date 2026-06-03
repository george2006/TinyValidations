using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TinyValidations.SourceGen;
using Xunit;

namespace TinyValidations.SourceGen.Tests;

internal static class SourceGeneratorTestHost
{
    public static SourceGeneratorRun Run(string source)
    {
        var compilation = CSharpCompilation.Create(
            "Tests",
            new[] { CSharpSyntaxTree.ParseText(source) },
            GetMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValidationSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out _);

        return new SourceGeneratorRun(driver.GetRunResult(), outputCompilation);
    }

    private static MetadataReference[] GetMetadataReferences()
    {
        var trustedAssemblies =
            ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        return trustedAssemblies
            .Concat(new[]
            {
                typeof(TinyValidations.IValidation<>).Assembly.Location,
                typeof(Microsoft.Extensions.DependencyInjection.ServiceDescriptor).Assembly.Location,
                typeof(Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions).Assembly.Location
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }
}

internal sealed class SourceGeneratorRun
{
    private readonly GeneratorDriverRunResult _result;
    private readonly Compilation _compilation;

    public SourceGeneratorRun(GeneratorDriverRunResult result, Compilation compilation)
    {
        _result = result;
        _compilation = compilation;
    }

    public Diagnostic SingleDiagnostic()
    {
        return Assert.Single(_result.Diagnostics);
    }

    public string SingleGeneratedSource()
    {
        var generated = Assert.Single(_result.GeneratedTrees);
        return generated.GetText().ToString();
    }

    public void ShouldHaveNoDiagnostics()
    {
        Assert.Empty(_result.Diagnostics);
    }

    public void ShouldHaveNoCompilationErrors()
    {
        var errors = _compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        Assert.Empty(errors);
    }

    public void ShouldGenerateNoSource()
    {
        Assert.Empty(_result.GeneratedTrees);
    }
}
