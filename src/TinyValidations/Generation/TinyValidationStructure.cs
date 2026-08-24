using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyValidations;

public sealed class TinyValidationStructure
{
    public TinyValidationStructure(
        Type validatedType,
        Type declarationType,
        IEnumerable<TinyValidationRuleStructure> rules,
        IEnumerable<Type> customRuleTypes)
    {
        if (validatedType is null)
        {
            throw new ArgumentNullException(nameof(validatedType));
        }

        if (declarationType is null)
        {
            throw new ArgumentNullException(nameof(declarationType));
        }

        if (rules is null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        if (customRuleTypes is null)
        {
            throw new ArgumentNullException(nameof(customRuleTypes));
        }

        var copiedCustomRuleTypes = customRuleTypes
            .Select(type => type ?? throw new ArgumentException(
                "Custom rule types cannot contain null values.",
                nameof(customRuleTypes)))
            .ToArray();

        ValidatedType = validatedType;
        DeclarationType = declarationType;
        Rules = Array.AsReadOnly(rules.ToArray());
        CustomRuleTypes = Array.AsReadOnly(copiedCustomRuleTypes);
        ValidatedTypeIdentity = GetIdentity(ValidatedType);
        DeclarationIdentity = GetIdentity(DeclarationType);
        CustomRuleIdentities = Array.AsReadOnly(
            copiedCustomRuleTypes
                .Select(GetIdentity)
                .ToArray());
    }

    public Type ValidatedType { get; }

    public Type DeclarationType { get; }

    public string ValidatedTypeIdentity { get; }

    public string DeclarationIdentity { get; }

    public IReadOnlyList<TinyValidationRuleStructure> Rules { get; }

    public int BuiltInRuleCount => Rules.Count;

    public IReadOnlyList<Type> CustomRuleTypes { get; }

    public IReadOnlyList<string> CustomRuleIdentities { get; }

    private static string GetIdentity(Type type)
    {
        return type.FullName ?? type.Name;
    }
}
