using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class ConstantArgumentFormatter
    {
        public string FormatExpression(object? value, ITypeSymbol? type = null)
        {
            var argument = CreateConstantExpression(value);
            if (!RequiresExplicitType(type, value))
            {
                return argument;
            }

            var typeName = type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return "(" + typeName + ")(" + argument + ")";
        }

        public string FormatDisplayText(object? value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is string text)
            {
                return text;
            }

            if (value is char character)
            {
                return character.ToString();
            }

            return Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatPrimitive(
                value,
                quoteStrings: false,
                useHexadecimalNumbers: false);
        }

        private static string CreateConstantExpression(object? value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is decimal decimalValue)
            {
                var literal = SyntaxFactory.Literal(decimalValue);
                return literal.Text;
            }

            if (value is float floatValue)
            {
                var literal = SyntaxFactory.Literal(floatValue);
                return CreateFloatingPointArgument(floatValue, literal.Text, "float");
            }

            if (value is double doubleValue)
            {
                var literal = SyntaxFactory.Literal(doubleValue);
                var argument = literal.Text + "D";
                return CreateFloatingPointArgument(doubleValue, argument, "double");
            }

            return Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatPrimitive(
                value,
                quoteStrings: true,
                useHexadecimalNumbers: false);
        }

        private static string CreateFloatingPointArgument(double value, string literal, string typeName)
        {
            if (double.IsNaN(value))
            {
                return typeName + ".NaN";
            }

            if (double.IsPositiveInfinity(value))
            {
                return typeName + ".PositiveInfinity";
            }

            if (double.IsNegativeInfinity(value))
            {
                return typeName + ".NegativeInfinity";
            }

            return literal;
        }

        private static bool RequiresExplicitType(ITypeSymbol? type, object? value)
        {
            if (type == null)
            {
                return false;
            }

            if (value == null)
            {
                return true;
            }

            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Char:
                case SpecialType.System_Int32:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Decimal:
                case SpecialType.System_String:
                    return false;
                default:
                    return true;
            }
        }
    }
}
