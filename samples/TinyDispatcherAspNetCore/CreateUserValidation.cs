using TinyValidations;

namespace TinyDispatcherAspNetCore;

public sealed class CreateUserValidation : IValidation<CreateUser>
{
    public void Define(ValidationRules<CreateUser> rules)
    {
        rules.Required(x => x.Email);
        rules.Email(x => x.Email);
        rules.TextLengthAtLeast(x => x.Name, 2);
        rules.AtLeast(x => x.Age, 18);
        rules.Use<UniqueEmailRule>();
    }
}
