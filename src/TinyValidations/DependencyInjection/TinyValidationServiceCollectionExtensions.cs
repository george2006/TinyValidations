using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TinyValidations;

public static class TinyValidationServiceCollectionExtensions
{
    public static IServiceCollection UseTinyValidations(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAddScoped<ITinyValidator, TinyValidator>();
        TinyValidationBootstrap.Apply(services);
        return services;
    }
}
