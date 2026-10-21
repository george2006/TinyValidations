using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class RuleArgumentAnalyzer
    {
        public ArgumentSyntax? FindArgument(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocation,
            int parameterIndex)
        {
            var operation = semanticModel.GetOperation(invocation);
            if (!(operation is IInvocationOperation call))
            {
                return null;
            }

            foreach (var argument in call.Arguments)
            {
                if (argument.ArgumentKind != ArgumentKind.Explicit)
                {
                    continue;
                }

                if (argument.Parameter?.Ordinal == parameterIndex)
                {
                    return argument.Syntax.FirstAncestorOrSelf<ArgumentSyntax>();
                }
            }

            return null;
        }

        public string GetRuleArgument(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation)
        {
            if (!RuleShape.RequiresValueArgument(kind))
            {
                return string.Empty;
            }

            var argumentIndex = RuleShape.ValueArgumentIndex(kind);
            var argument = FindArgument(semanticModel, invocation, argumentIndex);
            if (argument == null)
            {
                return string.Empty;
            }

            return GetConstantArgument(semanticModel, argument.Expression);
        }

        public string GetMessage(
            SemanticModel semanticModel,
            ExpressionSyntax expression)
        {
            var constant = semanticModel.GetConstantValue(expression);
            return CreateConstantArgument(constant.Value);
        }

        public string GetRuleArgumentDisplay(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax invocation)
        {
            if (!RuleShape.RequiresValueArgument(kind))
            {
                return string.Empty;
            }

            var argumentIndex = RuleShape.ValueArgumentIndex(kind);
            var argument = FindArgument(semanticModel, invocation, argumentIndex);
            if (argument == null)
            {
                return string.Empty;
            }

            var constant = semanticModel.GetConstantValue(argument.Expression);

            if (constant.Value == null)
            {
                return "null";
            }

            if (constant.Value is string text)
            {
                return text;
            }

            if (constant.Value is char character)
            {
                return character.ToString();
            }

            return Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatPrimitive(
                constant.Value,
                quoteStrings: false,
                useHexadecimalNumbers: false);
        }

        private static string CreateConstantArgument(object? value)
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

        private static string GetConstantArgument(
            SemanticModel semanticModel,
            ExpressionSyntax expression)
        {
            var constant = semanticModel.GetConstantValue(expression);

            var argument = CreateConstantArgument(constant.Value);
            var type = semanticModel.GetTypeInfo(expression).Type;
            if (!RequiresExplicitType(type, constant.Value))
            {
                return argument;
            }

            var typeName = type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return "(" + typeName + ")(" + argument + ")";
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
