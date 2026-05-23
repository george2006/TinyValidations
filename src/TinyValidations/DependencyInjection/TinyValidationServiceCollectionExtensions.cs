using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TinyValidations;

public static class TinyValidationServiceCollectionExtensions
{
    public static IServiceCollection UseTinyValidations(this IServiceCollection services)
    {
        services.TryAddScoped<ITinyValidator, TinyValidator>();
        TinyValidationBootstrap.Apply(services);
        return services;
    }
}
