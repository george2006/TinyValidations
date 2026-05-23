using Microsoft.CodeAnalysis;

namespace TinyValidations.SourceGen.Validation
{
    internal static class ValidationDiagnostics
    {
        public static readonly DiagnosticDescriptor WrongDefineSignature = new DiagnosticDescriptor(
            "TV0001",
            "Validation declaration has no usable Define method",
            "Validation declaration '{0}' must contain a Define method with one rules parameter",
            "TinyValidations",
            DiagnosticSeverity.Warning,
            true);

        public static readonly DiagnosticDescriptor UnsupportedRuleCall = new DiagnosticDescriptor(
            "TV0002",
            "Validation rule call is not supported",
            "Validation rule call '{0}' is not supported by TinyValidations",
            "TinyValidations",
            DiagnosticSeverity.Warning,
            true);

        public static readonly DiagnosticDescriptor UnsupportedSelector = new DiagnosticDescriptor(
            "TV0003",
            "Validation rule selector is not supported",
            "Validation rule selector '{0}' must be a simple member access expression",
            "TinyValidations",
            DiagnosticSeverity.Warning,
            true);

        public static readonly DiagnosticDescriptor UnsupportedArgument = new DiagnosticDescriptor(
            "TV0004",
            "Validation rule argument is not supported",
            "Validation rule argument '{0}' must be a constant value",
            "TinyValidations",
            DiagnosticSeverity.Warning,
            true);

        public static readonly DiagnosticDescriptor InvalidCustomRule = new DiagnosticDescriptor(
            "TV0005",
            "Custom validation rule type is invalid",
            "Custom validation rule '{0}' must implement IAsyncValidationRule<T>",
            "TinyValidations",
            DiagnosticSeverity.Warning,
            true);

        public static readonly DiagnosticDescriptor EmptyValidation = new DiagnosticDescriptor(
            "TV0006",
            "Validation declaration has no rules",
            "Validation declaration '{0}' does not contain validation rules",
            "TinyValidations",
            DiagnosticSeverity.Warning,
            true);
    }
}
