using System.Text;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal static class CustomRuleFieldName
    {
        public static string Create(string typeName)
        {
            if (typeName.StartsWith("global::"))
            {
                typeName = typeName.Substring("global::".Length);
            }

            var builder = new StringBuilder("_");

            foreach (var character in typeName)
            {
                if (IsNameCharacter(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
            }

            return builder.ToString();
        }

        private static bool IsNameCharacter(char character)
        {
            if (IsUppercaseLetter(character))
            {
                return true;
            }

            if (IsLowercaseLetter(character))
            {
                return true;
            }

            return IsDigit(character);
        }

        private static bool IsUppercaseLetter(char character)
        {
            if (character < 'A')
            {
                return false;
            }

            return character <= 'Z';
        }

        private static bool IsLowercaseLetter(char character)
        {
            if (character < 'a')
            {
                return false;
            }

            return character <= 'z';
        }

        private static bool IsDigit(char character)
        {
            if (character < '0')
            {
                return false;
            }

            return character <= '9';
        }
    }
}

