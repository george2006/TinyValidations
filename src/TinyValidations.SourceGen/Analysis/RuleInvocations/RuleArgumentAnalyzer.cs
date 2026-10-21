using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class RuleArgumentAnalyzer
    {
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
            return GetArgument(semanticModel, invocation, argumentIndex);
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
            var expression = invocation.ArgumentList.Arguments[argumentIndex].Expression;
            var constant = semanticModel.GetConstantValue(expression);

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
                return CreateFloatingPointArgument(doubleValue, literal.Text, "double");
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

        private static string GetArgument(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocation,
            int argumentIndex)
        {
            if (!HasArgument(invocation, argumentIndex))
            {
                return string.Empty;
            }

            var expression = invocation.ArgumentList.Arguments[argumentIndex].Expression;
            var constant = semanticModel.GetConstantValue(expression);

            return CreateConstantArgument(constant.Value);
        }

        private static bool HasArgument(InvocationExpressionSyntax invocation, int argumentIndex)
        {
            return invocation.ArgumentList.Arguments.Count > argumentIndex;
        }
    }
}
