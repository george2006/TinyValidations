using System.Collections.Generic;

namespace TinyValidations;

public interface ITinyValidationStructureContribution
{
    IReadOnlyList<TinyValidationStructure> Validations { get; }
}
