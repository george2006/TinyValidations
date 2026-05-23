using System.Threading;
using System.Threading.Tasks;

namespace TinyValidations;

public interface IAsyncValidationRule<T>
{
    ValueTask ValidateAsync(T instance, ValidationErrorCollection errors, CancellationToken cancellationToken);
}
