using Xunit;

namespace TinyValidations.SourceGen.Tests;

public sealed class RequiredCompilationTests
{
    [Fact]
    public void Generates_compilable_validation_for_representative_CLR_type_families()
    {
        var source = """
#nullable enable
using TinyValidations;

public sealed class RequiredTypeFamiliesValidation : IValidation<RequiredTypeFamilies>
{
    public void Define(ValidationRules<RequiredTypeFamilies> rules)
    {
        rules.Required(value => value.Text);
        rules.Required(value => value.Reference);
        rules.Required(value => value.Contract);
        rules.Required(value => value.Array);
        rules.Required(value => value.Collection);
        rules.Required(value => value.Identifier);
        rules.Required(value => value.OptionalIdentifier);
        rules.Required(value => value.Integer);
        rules.Required(value => value.OptionalInteger);
        rules.Required(value => value.Byte);
        rules.Required(value => value.SignedByte);
        rules.Required(value => value.Short);
        rules.Required(value => value.UnsignedShort);
        rules.Required(value => value.UnsignedInteger);
        rules.Required(value => value.Long);
        rules.Required(value => value.UnsignedLong);
        rules.Required(value => value.Decimal);
        rules.Required(value => value.Float);
        rules.Required(value => value.Double);
        rules.Required(value => value.Boolean);
        rules.Required(value => value.Character);
        rules.Required(value => value.Timestamp);
        rules.Required(value => value.TimestampWithOffset);
        rules.Required(value => value.Duration);
        rules.Required(value => value.Date);
        rules.Required(value => value.Time);
        rules.Required(value => value.Status);
        rules.Required(value => value.OptionalStatus);
        rules.Required(value => value.Structure);
        rules.Required(value => value.OptionalStructure);
        rules.Required(value => value.RecordStructure);
        rules.Required(value => value.Tuple);
        rules.Required(value => value.Pair);
        rules.Required(value => value.Uri);
        rules.Required(value => value.GenericReference);
        rules.Required(value => value.Nested!.Identifier);
    }
}

public sealed class RequiredTypeFamilies
{
    public string? Text { get; init; }
    public RequiredReference? Reference { get; init; }
    public IRequiredContract? Contract { get; init; }
    public string[]? Array { get; init; }
    public System.Collections.Generic.IReadOnlyList<string>? Collection { get; init; }
    public System.Guid Identifier { get; init; }
    public System.Guid? OptionalIdentifier { get; init; }
    public int Integer { get; init; }
    public int? OptionalInteger { get; init; }
    public byte Byte { get; init; }
    public sbyte SignedByte { get; init; }
    public short Short { get; init; }
    public ushort UnsignedShort { get; init; }
    public uint UnsignedInteger { get; init; }
    public long Long { get; init; }
    public ulong UnsignedLong { get; init; }
    public decimal Decimal { get; init; }
    public float Float { get; init; }
    public double Double { get; init; }
    public bool Boolean { get; init; }
    public char Character { get; init; }
    public System.DateTime Timestamp { get; init; }
    public System.DateTimeOffset TimestampWithOffset { get; init; }
    public System.TimeSpan Duration { get; init; }
    public System.DateOnly Date { get; init; }
    public System.TimeOnly Time { get; init; }
    public RequiredStatus Status { get; init; }
    public RequiredStatus? OptionalStatus { get; init; }
    public RequiredStructure Structure { get; init; }
    public RequiredStructure? OptionalStructure { get; init; }
    public RequiredRecordStructure RecordStructure { get; init; }
    public (int Number, string? Text) Tuple { get; init; }
    public System.Collections.Generic.KeyValuePair<string, int> Pair { get; init; }
    public System.Uri? Uri { get; init; }
    public RequiredGenericReference<int>? GenericReference { get; init; }
    public RequiredNestedReference? Nested { get; init; }
}

public sealed class RequiredReference;

public interface IRequiredContract;

public sealed class RequiredGenericReference<T>;

public sealed class RequiredNestedReference
{
    public System.Guid Identifier { get; init; }
}

public enum RequiredStatus
{
    None,
    Ready
}

public readonly struct RequiredStructure(int value)
{
    public int Value { get; } = value;
}

public readonly record struct RequiredRecordStructure(int Value);
""";

        var result = SourceGeneratorTestHost.Run(source);

        result.ShouldHaveNoDiagnostics();
        result.ShouldHaveNoCompilationErrors();
    }
}
