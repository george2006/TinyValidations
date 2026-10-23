using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class RequiresRuleInvocation
    {
        public RequiresRuleInvocation(
            SemanticModel semanticModel,
            InvocationExpressionSyntax syntax)
        {
            Syntax = syntax;

            var call = semanticModel.GetOperation(syntax) as IInvocationOperation;
            var argumentBinder = new RuleArgumentBinder();

            SelectorArgument = argumentBinder.FindArgument(call, 0);
            RequirementArgument = argumentBinder.FindArgument(call, 1);
            MessageArgument = argumentBinder.FindArgument(call, 2);

            if (RequirementArgument != null)
            {
                RequirementMethod = ResolveRequirementMethod(semanticModel, RequirementArgument.Expression);
            }

            if (MessageArgument != null)
            {
                MessageConstant = semanticModel.GetConstantValue(MessageArgument.Expression);
            }
        }

        public InvocationExpressionSyntax Syntax { get; }

        public ArgumentSyntax? SelectorArgument { get; }

        public ArgumentSyntax? RequirementArgument { get; }

        public ArgumentSyntax? MessageArgument { get; }

        public IMethodSymbol? RequirementMethod { get; }

        public Optional<object?> MessageConstant { get; }

        private static IMethodSymbol? ResolveRequirementMethod(
            SemanticModel semanticModel,
            ExpressionSyntax expression)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(expression);
            var symbol = symbolInfo.Symbol ?? GetSingleCandidate(symbolInfo);
            return symbol as IMethodSymbol;
        }

        private static ISymbol? GetSingleCandidate(SymbolInfo symbolInfo)
        {
            if (symbolInfo.CandidateSymbols.Length != 1)
            {
                return null;
            }

            return symbolInfo.CandidateSymbols[0];
        }
    }
}
