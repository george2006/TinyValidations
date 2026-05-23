using System.Text;

namespace TinyValidations.SourceGen.Emission.Writing
{
    internal sealed class SourceWriter
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private int _indent;

        public void WriteLine(string line = "")
        {
            if (line.Length > 0)
            {
                _builder.Append(new string(' ', _indent * 4));
            }

            _builder.AppendLine(line);
        }

        public void OpenBlock()
        {
            WriteLine("{");
            _indent++;
        }

        public void CloseBlock()
        {
            _indent--;
            WriteLine("}");
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
