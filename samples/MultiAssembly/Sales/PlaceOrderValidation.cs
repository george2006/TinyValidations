using TinyValidations;

namespace Sales;

public sealed class PlaceOrderValidation : IValidation<PlaceOrder>
{
    public void Define(ValidationRules<PlaceOrder> rules)
    {
        rules.Required(x => x.CustomerEmail);
        rules.Email(x => x.CustomerEmail);
        rules.HasItems(x => x.Lines);
        rules.Above(x => x.Total, 0m);
        rules.Use<CustomerCanOrderRule>();
    }
}
