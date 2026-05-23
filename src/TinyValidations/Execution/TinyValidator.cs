using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TinyValidations;

internal sealed class TinyValidator : ITinyValidator
{
    private readonly IServiceProvider _serviceProvider;

    public TinyValidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<ValidationResult> ValidateAsync<T>(T instance, CancellationToken cancellationToken = default)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        var errors = new ValidationErrorCollection();
        var runners = _serviceProvider.GetServices<ITinyValidationRunner<T>>();

        foreach (var runner in runners)
        {
            var result = await runner.ValidateAsync(instance, cancellationToken).ConfigureAwait(false);
            errors.AddRange(result.Errors);
        }

        return errors.ToResult();
    }
}
