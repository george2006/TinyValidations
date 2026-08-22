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
        if (rules is null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        if (customRuleTypes is null)
        {
            throw new ArgumentNullException(nameof(customRuleTypes));
        }

        ValidatedTypeIdentity = GetIdentity(validatedType, nameof(validatedType));
        DeclarationIdentity = GetIdentity(declarationType, nameof(declarationType));
        Rules = Array.AsReadOnly(rules.ToArray());
        CustomRuleIdentities = Array.AsReadOnly(
            customRuleTypes
                .Select(type => GetIdentity(type, nameof(customRuleTypes)))
                .ToArray());
    }

    public string ValidatedTypeIdentity { get; }

    public string DeclarationIdentity { get; }

    public IReadOnlyList<TinyValidationRuleStructure> Rules { get; }

    public int BuiltInRuleCount => Rules.Count;

    public IReadOnlyList<string> CustomRuleIdentities { get; }

    private static string GetIdentity(Type? type, string parameterName)
    {
        if (type is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        return type.FullName ?? type.Name;
    }
}
