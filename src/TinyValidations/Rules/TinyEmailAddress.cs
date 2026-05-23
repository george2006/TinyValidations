using System.Text.RegularExpressions;

namespace TinyValidations;

public static class TinyEmailAddress
{
    private static readonly Regex EmailExpression = new Regex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool IsValid(string value)
    {
        return EmailExpression.IsMatch(value);
    }
}
