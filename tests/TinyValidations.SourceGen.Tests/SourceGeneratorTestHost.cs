using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TinyValidations.SourceGen;
using Xunit;

namespace TinyValidations.SourceGen.Tests;

internal static class SourceGeneratorTestHost
{
    public static GeneratorDriverRunResult Run(string source)
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

    public static Diagnostic GetSingleDiagnostic(string source)
    {
        var result = Run(source);
        return Assert.Single(result.Diagnostics);
    }
}
