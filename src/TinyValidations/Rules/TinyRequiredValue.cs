using System.Collections.Generic;

namespace TinyValidations;

public static class TinyRequiredValue
{
    public static bool IsMissing<TValue>(TValue value)
    {
        if (value is string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        return EqualityComparer<TValue>.Default.Equals(
            value,
            default(TValue));
    }
}
