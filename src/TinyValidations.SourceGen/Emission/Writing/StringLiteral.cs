namespace TinyValidations.SourceGen.Emission.Writing
{
    internal static class StringLiteral
    {
        public static string Create(string value)
        {
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
    }
}
