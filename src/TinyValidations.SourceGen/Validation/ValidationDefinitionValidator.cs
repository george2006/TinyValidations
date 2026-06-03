using System;
using Microsoft.CodeAnalysis;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Validation
{
    internal sealed class ValidationDefinitionValidator
    {
        public bool Validate(
            ValidationDefinitionSet definitions,
            Action<Diagnostic> reportDiagnostic)
        {
            var canGenerate = true;

            foreach (var issue in definitions.Issues)
            {
                ReportIssue(issue, reportDiagnostic);
                canGenerate = false;
            }

            if (definitions.Issues.Count > 0)
            {
                return false;
            }

            foreach (var definition in definitions.Validations)
            {
                if (HasRules(definition))
                {
                    continue;
                }

                ReportEmptyValidation(definition, reportDiagnostic);
                canGenerate = false;
            }

            return canGenerate;
        }

        private static bool HasRules(ValidationDefinition definition)
        {
            return definition.Rules.Count > 0;
        }

        private static void ReportEmptyValidation(
            ValidationDefinition definition,
            Action<Diagnostic> reportDiagnostic)
        {
            var diagnostic = Diagnostic.Create(
                ValidationDiagnostics.EmptyValidation,
                definition.DeclarationLocation,
                definition.ValidationTypeName);

            reportDiagnostic(diagnostic);
        }

        private static void ReportIssue(
            ValidationIssue issue,
            Action<Diagnostic> reportDiagnostic)
        {
            var diagnostic = Diagnostic.Create(
                issue.Descriptor,
                issue.Location,
                issue.Value);

            reportDiagnostic(diagnostic);
        }
    }
}
