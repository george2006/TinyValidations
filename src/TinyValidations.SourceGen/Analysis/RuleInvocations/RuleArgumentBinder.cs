using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class RuleArgumentBinder
    {
        public ArgumentSyntax? FindArgument(
            IInvocationOperation? invocation,
            int parameterOrdinal)
        {
            if (invocation == null)
            {
                return null;
            }

            foreach (var argument in invocation.Arguments)
            {
                if (argument.ArgumentKind != ArgumentKind.Explicit)
                {
                    continue;
                }

                if (argument.Parameter?.Ordinal == parameterOrdinal)
                {
                    return argument.Syntax.FirstAncestorOrSelf<ArgumentSyntax>();
                }
            }

            return null;
        }
    }
}
