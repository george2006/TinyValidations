using Microsoft.Extensions.DependencyInjection;

namespace Sales;

public static class SalesServiceCollectionExtensions
{
    public static IServiceCollection AddSalesSample(this IServiceCollection services)
    {
        services.AddScoped<CustomerAccountStore>();
        return services;
    }
}
