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
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(TinyValidations.IValidation<>).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValidationSourceGenerator());
        driver = driver.RunGenerators(compilation);

        return new SourceGeneratorRun(driver.GetRunResult());
    }
}

internal sealed class SourceGeneratorRun
{
    private readonly GeneratorDriverRunResult _result;

    public SourceGeneratorRun(GeneratorDriverRunResult result)
    {
        _result = result;
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

    public void ShouldGenerateNoSource()
    {
        Assert.Empty(_result.GeneratedTrees);
    }
}
