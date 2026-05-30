using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class MemberAccessAnalyzer
    {
        public AnalyzedMemberAccess? Analyze(ExpressionSyntax expression)
        {
            if (!(expression is LambdaExpressionSyntax lambda))
            {
                return null;
            }

            var parameterName = GetParameterName(lambda);
            if (parameterName.Length == 0)
            {
                return null;
            }

            var members = ReadMembers(lambda.Body, parameterName);
            if (members.Count == 0)
            {
                return null;
            }

            var path = string.Join(".", members);
            return new AnalyzedMemberAccess(path, CreateAccess(members));
        }

        private static string GetParameterName(LambdaExpressionSyntax lambda)
        {
            if (lambda is SimpleLambdaExpressionSyntax simple)
            {
                return simple.Parameter.Identifier.ValueText;
            }

            if (lambda is ParenthesizedLambdaExpressionSyntax parenthesized)
            {
                return GetParameterName(parenthesized);
            }

            return string.Empty;
        }

        private static string GetParameterName(ParenthesizedLambdaExpressionSyntax lambda)
        {
            if (!HasSingleParameter(lambda))
            {
                return string.Empty;
            }

            return lambda.ParameterList.Parameters[0].Identifier.ValueText;
        }

        private static bool HasSingleParameter(ParenthesizedLambdaExpressionSyntax lambda)
        {
            return lambda.ParameterList.Parameters.Count == 1;
        }

        private static List<string> ReadMembers(SyntaxNode body, string parameterName)
        {
            var members = new List<string>();
            ExpressionSyntax? current = body as ExpressionSyntax;

            while (current is MemberAccessExpressionSyntax memberAccess)
            {
                members.Insert(0, memberAccess.Name.Identifier.ValueText);
                current = memberAccess.Expression;
            }

            if (IsOriginalParameter(current, parameterName))
            {
                return members;
            }

            return new List<string>();
        }

        private static bool IsOriginalParameter(ExpressionSyntax? expression, string parameterName)
        {
            if (!(expression is IdentifierNameSyntax identifier))
            {
                return false;
            }

            return identifier.Identifier.ValueText == parameterName;
        }

        private static string CreateAccess(List<string> members)
        {
            if (members.Count == 1)
            {
                return "instance." + members[0];
            }

            return "instance." + string.Join("?.", members);
        }
    }
}
