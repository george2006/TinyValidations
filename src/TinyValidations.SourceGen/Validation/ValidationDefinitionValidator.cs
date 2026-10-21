using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Validation
{
    internal sealed class ValidationDefinitionValidator
    {
        public ValidationDefinitionSet Validate(
            ValidationDefinitionSet definitions,
            Action<Diagnostic> reportDiagnostic)
        {
            foreach (var issue in definitions.Issues)
            {
                ReportIssue(issue, reportDiagnostic);
            }

            var validDefinitions = new List<ValidationDefinition>();

            foreach (var definition in definitions.Validations)
            {
                if (!HasRules(definition))
                {
                    ReportEmptyValidation(definition, reportDiagnostic);
                    continue;
                }

                validDefinitions.Add(definition);
            }

            return new ValidationDefinitionSet(validDefinitions, definitions.Issues);
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
