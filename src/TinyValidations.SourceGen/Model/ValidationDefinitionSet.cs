using System.Collections.Generic;

namespace TinyValidations.SourceGen.Model
{
    internal sealed class ValidationDefinitionSet
    {
        public ValidationDefinitionSet(
            IReadOnlyList<ValidationDefinition> validations,
            IReadOnlyList<ValidationIssue> issues)
        {
            Validations = validations;
            Issues = issues;
        }

        public IReadOnlyList<ValidationDefinition> Validations { get; }

        public IReadOnlyList<ValidationIssue> Issues { get; }
    }
}
