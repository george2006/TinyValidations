using Microsoft.Extensions.DependencyInjection;

namespace Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentitySample(this IServiceCollection services)
    {
        services.AddScoped<UserDirectory>();
        return services;
    }
}
