using Microsoft.CodeAnalysis;
using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class ConstantConversionTests
{
    public static IEnumerable<object[]> ConstantTypes()
    {
        var types = new[]
        {
            new[] { "bool", "(bool)true" },
            new[] { "char", "(char)10" },
            new[] { "sbyte", "(sbyte)10" },
            new[] { "byte", "(byte)10" },
            new[] { "short", "(short)10" },
            new[] { "ushort", "(ushort)10" },
            new[] { "int", "(int)10" },
            new[] { "uint", "(uint)10" },
            new[] { "uint", "10U" },
            new[] { "long", "(long)10" },
            new[] { "long", "10L" },
            new[] { "ulong", "(ulong)10" },
            new[] { "ulong", "10UL" },
            new[] { "nint", "(nint)10" },
            new[] { "nuint", "(nuint)10" },
            new[] { "float", "(float)10" },
            new[] { "float", "-0F" },
            new[] { "double", "(double)10" },
            new[] { "double", "10D" },
            new[] { "double", "-0D" },
            new[] { "decimal", "(decimal)10" },
            new[] { "string", "(string)\"10\"" },
            new[] { "string", "(string)null" },
            new[] { "Level", "(Level)10" },
            new[] { "System.Version", "(System.Version)null" }
        };

        var rules = new[] { "Above", "AtLeast", "Below", "AtMost" };

        foreach (var type in types)
        {
            var typeName = type[0];
            var expression = type[1];
            var supportsConstantField = typeName != "nint" && typeName != "nuint";

            foreach (var rule in rules)
            {
                yield return new object[] { typeName, expression, rule, false };
                if (supportsConstantField)
                {
                    yield return new object[] { typeName, expression, rule, true };
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(ConstantTypes))]
    public async Task Comparison_rules_preserve_conversions_for_constant_types(
        string type,
        string expression,
        string rule,
        bool useConstantField)
    {
        var argument = useConstantField ? "Threshold" : expression;
        var constantDeclaration = useConstantField
            ? "private const " + type + " Threshold = " + expression + ";"
            : string.Empty;
        var alternativeType = GetAlternativeType(type);
        var valueComparison = GetValueComparison(type);
        var comparison = GetComparison(rule);
        var source = $$"""
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinyValidations;

public enum Level { Minimum = 10 }

public readonly record struct Amount(int Value) : IComparable<Amount>
{
    public int CompareTo(Amount other) => Value.CompareTo(other.Value);
    public static implicit operator Amount({{type}} value)
    {
        var expectedValue = {{expression}};
        var matches = {{valueComparison}};
        return new Amount(matches ? 10 : 1000);
    }

    public static implicit operator Amount({{alternativeType}} value) => new Amount(1000);
}

public sealed record CreateItem(Amount Value);

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    {{constantDeclaration}}

    public void Define(ValidationRules<CreateItem> rules)
    {
        rules.{{rule}}<Amount>(x => x.Value, {{argument}});
    }

    public static bool Expected(int value)
    {
        var amount = new Amount(value);
        return Comparer<Amount>.Default.Compare(amount, {{argument}}) {{comparison}} 0;
    }

    public static async Task<bool> Generated(int value)
    {
        var runner = new TinyValidations.Generated.CreateItemValidationRunner();
        var amount = new Amount(value);
        var command = new CreateItem(amount);
        var result = await runner.ValidateAsync(command, default);
        return result.IsValid;
    }
}
""";

        var result = SourceGeneratorTestHost.Run(source);

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();

        var assembly = result.LoadAssembly();
        var validation = assembly.GetType("CreateItemValidation")!;
        var expected = validation.GetMethod("Expected")!.CreateDelegate<Func<int, bool>>();
        var generated = validation.GetMethod("Generated")!.CreateDelegate<Func<int, Task<bool>>>();

        foreach (var value in new[] { 9, 10, 11 })
        {
            var expectedValid = expected(value);
            var generatedValid = await generated(value);

            Assert.Equal(expectedValid, generatedValid);
        }
    }

    [Theory]
    [InlineData("DateTime", "default(DateTime)")]
    [InlineData("DateTimeOffset", "default(DateTimeOffset)")]
    [InlineData("TimeSpan", "default(TimeSpan)")]
    [InlineData("DateOnly", "default(DateOnly)")]
    [InlineData("TimeOnly", "default(TimeOnly)")]
    [InlineData("Guid", "default(Guid)")]
    [InlineData("Half", "(Half)10")]
    [InlineData("Int128", "(Int128)10")]
    [InlineData("UInt128", "(UInt128)10")]
    [InlineData("System.Numerics.BigInteger", "default(System.Numerics.BigInteger)")]
    [InlineData("Version", "new Version(1, 0)")]
    public void Comparison_rules_reject_comparable_arguments_that_are_not_constants(string type, string argument)
    {
        foreach (var rule in new[] { "Above", "AtLeast", "Below", "AtMost" })
        {
            var source = $$"""
using System;
using TinyValidations;

public sealed record CreateItem({{type}} Value);

public sealed class CreateItemValidation : IValidation<CreateItem>
{
    public void Define(ValidationRules<CreateItem> rules)
    {
        rules.{{rule}}(x => x.Value, {{argument}});
    }
}
""";

            var result = SourceGeneratorTestHost.Run(source);
            var diagnostic = result.SingleDiagnostic();

            result.ShouldHaveNoCompilationErrors();
            result.ShouldGenerateNoSource();
            Assert.Equal("TV0004", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        }
    }

    private static string GetValueComparison(string type)
    {
        if (type == "float")
        {
            return "BitConverter.SingleToInt32Bits(value) == BitConverter.SingleToInt32Bits(expectedValue)";
        }

        if (type == "double")
        {
            return "BitConverter.DoubleToInt64Bits(value) == BitConverter.DoubleToInt64Bits(expectedValue)";
        }

        return "EqualityComparer<" + type + ">.Default.Equals(value, expectedValue)";
    }

    private static string GetAlternativeType(string type)
    {
        if (type == "int")
        {
            return "long";
        }

        if (type == "string")
        {
            return "System.Version";
        }

        if (type == "System.Version")
        {
            return "string";
        }

        return "int";
    }

    private static string GetComparison(string rule)
    {
        switch (rule)
        {
            case "Above": return ">";
            case "AtLeast": return ">=";
            case "Below": return "<";
            case "AtMost": return "<=";
            default: throw new ArgumentOutOfRangeException(nameof(rule));
        }
    }
}
