using TinyValidations;

namespace Identity;

public sealed class RegisterUserValidation : IValidation<RegisterUser>
{
    public void Define(ValidationRules<RegisterUser> rules)
    {
        rules.Required(x => x.Email);
        rules.Email(x => x.Email);
        rules.TextLengthAtLeast(x => x.DisplayName, 2);
        rules.AtLeast(x => x.Age, 18);
        rules.Use<UniqueEmailRule>();
    }
}
