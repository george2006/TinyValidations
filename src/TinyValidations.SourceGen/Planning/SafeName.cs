using System.Text;

namespace TinyValidations.SourceGen.Planning
{
    internal static class SafeName
    {
        public static string Create(string value)
        {
            var builder = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                if (IsNameCharacter(character))
                {
                    builder.Append(character);
                }
                else
                {
                    builder.Append('_');
                }
            }

            return builder.ToString().Replace("global__", string.Empty);
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
