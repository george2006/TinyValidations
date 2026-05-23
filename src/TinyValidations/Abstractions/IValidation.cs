namespace TinyValidations;

public interface IValidation<T>
{
    void Define(ValidationRules<T> rules);
}
