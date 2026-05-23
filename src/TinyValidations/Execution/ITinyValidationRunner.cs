using System.Threading;
using System.Threading.Tasks;

namespace TinyValidations;

public interface ITinyValidationRunner<T>
{
    ValueTask<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken);
}
