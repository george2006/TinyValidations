using Microsoft.CodeAnalysis;

namespace TinyValidations.SourceGen.Model
{
    internal sealed class ValidationIssue
    {
        public ValidationIssue(DiagnosticDescriptor descriptor, Location location, string value)
        {
            Descriptor = descriptor;
            Location = location;
            Value = value;
        }

        public DiagnosticDescriptor Descriptor { get; }

        public Location Location { get; }

        public string Value { get; }
    }
}
