using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace TinyValidations.SourceGen.Model
{
    internal sealed class ValidationDefinition
    {
        public ValidationDefinition(
            Location declarationLocation,
            string validationTypeName,
            string commandTypeName,
            IReadOnlyList<RuleDefinition> rules)
        {
            DeclarationLocation = declarationLocation;
            ValidationTypeName = validationTypeName;
            CommandTypeName = commandTypeName;
            Rules = rules;
        }

        public Location DeclarationLocation { get; }

        public string ValidationTypeName { get; }

        public string CommandTypeName { get; }

        public IReadOnlyList<RuleDefinition> Rules { get; }
    }
}
