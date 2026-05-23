using TinyValidations;

namespace Sales;

public sealed class CustomerCanOrderRule : IAsyncValidationRule<PlaceOrder>
{
    private readonly CustomerAccountStore _accounts;

    public CustomerCanOrderRule(CustomerAccountStore accounts)
    {
        _accounts = accounts;
    }

    public ValueTask ValidateAsync(
        PlaceOrder instance,
        ValidationErrorCollection errors,
        CancellationToken cancellationToken)
    {
        if (_accounts.IsBlocked(instance.CustomerEmail))
        {
            errors.Add(nameof(PlaceOrder.CustomerEmail), "Customer cannot place orders.");
        }

        return ValueTask.CompletedTask;
    }
}
