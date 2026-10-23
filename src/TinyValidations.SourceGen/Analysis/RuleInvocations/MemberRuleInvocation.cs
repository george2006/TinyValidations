using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class MemberRuleInvocation
    {
        public MemberRuleInvocation(
            SemanticModel semanticModel,
            RuleKind kind,
            InvocationExpressionSyntax syntax)
        {
            Kind = kind;
            Syntax = syntax;

            var call = semanticModel.GetOperation(syntax) as IInvocationOperation;
            var argumentBinder = new RuleArgumentBinder();
            var valueParameterOrdinal = RuleShape.ValueParameterOrdinal(kind);
            var messageParameterOrdinal = RuleShape.MessageParameterOrdinal(kind);

            SelectorArgument = argumentBinder.FindArgument(call, 0);
            ValueArgument = argumentBinder.FindArgument(call, valueParameterOrdinal);
            MessageArgument = argumentBinder.FindArgument(call, messageParameterOrdinal);

            if (ValueArgument != null)
            {
                ValueConstant = semanticModel.GetConstantValue(ValueArgument.Expression);
                ValueType = semanticModel.GetTypeInfo(ValueArgument.Expression).Type;
            }

            if (MessageArgument != null)
            {
                MessageConstant = semanticModel.GetConstantValue(MessageArgument.Expression);
            }

            Method = semanticModel.GetSymbolInfo(syntax).Symbol as IMethodSymbol;
        }

        public RuleKind Kind { get; }

        public InvocationExpressionSyntax Syntax { get; }

        public ArgumentSyntax? SelectorArgument { get; }

        public ArgumentSyntax? ValueArgument { get; }

        public ArgumentSyntax? MessageArgument { get; }

        public Optional<object?> ValueConstant { get; }

        public ITypeSymbol? ValueType { get; }

        public Optional<object?> MessageConstant { get; }

        public IMethodSymbol? Method { get; }
    }
}
