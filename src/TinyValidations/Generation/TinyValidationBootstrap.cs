using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace TinyValidations;

public static class TinyValidationBootstrap
{
    private static readonly object SyncRoot = new object();
    private static readonly List<ITinyValidationContribution> Contributions = new List<ITinyValidationContribution>();

    public static void AddContribution(ITinyValidationContribution contribution)
    {
        if (contribution is null)
        {
            throw new ArgumentNullException(nameof(contribution));
        }

        lock (SyncRoot)
        {
            if (HasContribution(contribution))
            {
                return;
            }

            Contributions.Add(contribution);
        }
    }

    public static void Apply(IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (HasAppliedMarker(services))
        {
            return;
        }

        services.AddSingleton<TinyValidationBootstrapAppliedMarker>();

        ITinyValidationContribution[] snapshot;

        lock (SyncRoot)
        {
            snapshot = Contributions.ToArray();
        }

        foreach (var contribution in snapshot)
        {
            contribution.Register(services);
        }
    }

    public static IReadOnlyList<TinyValidationStructure> GetValidations()
    {
        var validations = new List<TinyValidationStructure>();
        ITinyValidationContribution[] snapshot;

        lock (SyncRoot)
        {
            snapshot = Contributions.ToArray();
        }

        foreach (var contribution in snapshot)
        {
            if (contribution is ITinyValidationStructureContribution structureContribution)
            {
                validations.AddRange(structureContribution.Validations);
            }
        }

        validations.Sort(CompareValidations);
        return validations.ToArray();
    }

    private static int CompareValidations(
        TinyValidationStructure left,
        TinyValidationStructure right)
    {
        var validatedTypeComparison = string.CompareOrdinal(
            left.ValidatedTypeIdentity,
            right.ValidatedTypeIdentity);

        return validatedTypeComparison != 0
            ? validatedTypeComparison
            : string.CompareOrdinal(left.DeclarationIdentity, right.DeclarationIdentity);
    }

    private static bool HasContribution(ITinyValidationContribution contribution)
    {
        foreach (var registered in Contributions)
        {
            if (ReferenceEquals(registered, contribution))
            {
                return true;
            }

            if (registered.GetType() == contribution.GetType())
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAppliedMarker(IServiceCollection services)
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == typeof(TinyValidationBootstrapAppliedMarker))
            {
                return true;
            }
        }

        return false;
    }

    private sealed class TinyValidationBootstrapAppliedMarker
    {
    }
}
