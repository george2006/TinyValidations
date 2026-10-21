namespace TinyValidations.SourceGen.Emission.Writing
{
    internal static class StringLiteral
    {
        public static string Create(string value)
        {
            var escapedValue = value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\u0085", "\\u0085")
                .Replace("\u2028", "\\u2028")
                .Replace("\u2029", "\\u2029");

            return "\"" + escapedValue + "\"";
        }
    }
}
