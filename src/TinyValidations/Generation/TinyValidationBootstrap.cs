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
            throw new System.ArgumentNullException(nameof(contribution));
        }

        lock (SyncRoot)
        {
            Contributions.Add(contribution);
        }
    }

    public static void Apply(IServiceCollection services)
    {
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
}
