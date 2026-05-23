using Microsoft.Extensions.DependencyInjection;

namespace TinyValidations;

public interface ITinyValidationContribution
{
    void Register(IServiceCollection services);
}
