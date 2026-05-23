using System.Threading;
using System.Threading.Tasks;

namespace TinyValidations;

public interface ITinyValidator
{
    ValueTask<ValidationResult> ValidateAsync<T>(T instance, CancellationToken cancellationToken = default);
}
