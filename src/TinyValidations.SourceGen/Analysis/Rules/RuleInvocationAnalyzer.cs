using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Validation;

namespace TinyValidations.SourceGen.Analysis.Rules
{
    internal sealed class RuleInvocationAnalyzer
    {
        private readonly RuleMethodMap _methodMap = new RuleMethodMap();
        private readonly MemberAccessAnalyzer _memberAccessAnalyzer = new MemberAccessAnalyzer();
        private readonly RuleArgumentAnalyzer _argumentAnalyzer = new RuleArgumentAnalyzer();

        public RuleAnalysisResult? Analyze(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocation,
            INamedTypeSymbol validationRules,
            INamedTypeSymbol commandType)
        {
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
            {
                return null;
            }

            if (!IsValidationRulesInvocation(semanticModel, memberAccess, invocation, validationRules))
            {
                return null;
            }

            var methodName = memberAccess.Name.Identifier.ValueText;
            var kind = _methodMap.GetKind(methodName);
            if (kind == null)
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedRuleCall,
                    memberAccess.Name.GetLocation(),
                    methodName));
            }

            if (kind.Value == RuleKind.Use)
            {
                return AnalyzeCustomRule(semanticModel, invocation, memberAccess.Name, commandType);
            }

            if (kind.Value == RuleKind.Requires)
            {
                return AnalyzeRequiresRule(semanticModel, invocation);
            }

            if (invocation.ArgumentList.Arguments.Count == 0)
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedSelector,
                    invocation.GetLocation(),
                    invocation.ToString()));
            }

            var member = _memberAccessAnalyzer.Analyze(invocation.ArgumentList.Arguments[0].Expression);
            if (member == null)
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedSelector,
                    invocation.ArgumentList.Arguments[0].GetLocation(),
                    invocation.ArgumentList.Arguments[0].Expression.ToString()));
            }

            if (HasUnsupportedArgument(kind.Value, invocation))
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedArgument,
                    invocation.GetLocation(),
                    invocation.ToString()));
            }

            var argument = _argumentAnalyzer.GetRuleArgument(kind.Value, invocation);
            var message = _argumentAnalyzer.GetMessage(kind.Value, invocation);

            return RuleAnalysisResult.ForRule(new RuleDefinition(kind.Value, member.Path, member.Access, argument, message, string.Empty));
        }

        private RuleAnalysisResult AnalyzeRequiresRule(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocation)
        {
            if (invocation.ArgumentList.Arguments.Count < 3)
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedArgument,
                    invocation.GetLocation(),
                    invocation.ToString()));
            }

            var member = _memberAccessAnalyzer.Analyze(invocation.ArgumentList.Arguments[0].Expression);
            if (member == null)
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedSelector,
                    invocation.ArgumentList.Arguments[0].GetLocation(),
                    invocation.ArgumentList.Arguments[0].Expression.ToString()));
            }

            if (!IsSupportedRequirementMethod(semanticModel, invocation.ArgumentList.Arguments[1].Expression, out var requirementMethod))
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedArgument,
                    invocation.ArgumentList.Arguments[1].GetLocation(),
                    invocation.ArgumentList.Arguments[1].Expression.ToString()));
            }

            if (!IsSupportedArgument(invocation, 2))
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.UnsupportedArgument,
                    invocation.ArgumentList.Arguments[2].GetLocation(),
                    invocation.ArgumentList.Arguments[2].Expression.ToString()));
            }

            var message = invocation.ArgumentList.Arguments[2].Expression.ToString();

            return RuleAnalysisResult.ForRule(new RuleDefinition(
                RuleKind.Requires,
                member.Path,
                member.Access,
                string.Empty,
                message,
                string.Empty,
                requirementMethod));
        }

        private static RuleAnalysisResult AnalyzeCustomRule(
            SemanticModel semanticModel,
            InvocationExpressionSyntax invocation,
            SimpleNameSyntax methodName,
            INamedTypeSymbol commandType)
        {
            if (!(methodName is GenericNameSyntax genericName))
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.InvalidCustomRule,
                    methodName.GetLocation(),
                    methodName.ToString()));
            }

            if (!HasSingleTypeArgument(genericName))
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.InvalidCustomRule,
                    genericName.GetLocation(),
                    genericName.ToString()));
            }

            var typeSyntax = genericName.TypeArgumentList.Arguments[0];
            var typeSymbol = semanticModel.GetTypeInfo(typeSyntax).Type;
            if (!IsValidCustomRule(typeSymbol, commandType))
            {
                return RuleAnalysisResult.ForIssue(new ValidationIssue(
                    ValidationDiagnostics.InvalidCustomRule,
                    typeSyntax.GetLocation(),
                    typeSyntax.ToString()));
            }

            var customRuleType = GetTypeName(typeSyntax, typeSymbol);

            return RuleAnalysisResult.ForRule(new RuleDefinition(RuleKind.Use, string.Empty, string.Empty, string.Empty, string.Empty, customRuleType));
        }

        private static bool HasSingleTypeArgument(GenericNameSyntax genericName)
        {
            return genericName.TypeArgumentList.Arguments.Count == 1;
        }

        private static string GetTypeName(TypeSyntax typeSyntax, ITypeSymbol? typeSymbol)
        {
            if (typeSymbol == null)
            {
                return typeSyntax.ToString();
            }

            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        private static bool IsValidationRulesInvocation(
            SemanticModel semanticModel,
            MemberAccessExpressionSyntax memberAccess,
            InvocationExpressionSyntax invocation,
            INamedTypeSymbol validationRules)
        {
            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;
            if (symbol is IMethodSymbol method)
            {
                return SymbolEqualityComparer.Default.Equals(method.ContainingType.OriginalDefinition, validationRules);
            }

            var expressionType = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
            if (!(expressionType is INamedTypeSymbol namedType))
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, validationRules);
        }

        private static bool HasUnsupportedArgument(RuleKind kind, InvocationExpressionSyntax invocation)
        {
            var valueArgumentIndex = GetValueArgumentIndex(kind);
            if (valueArgumentIndex >= 0)
            {
                if (!IsSupportedArgument(invocation, valueArgumentIndex))
                {
                    return true;
                }
            }

            var messageArgumentIndex = GetMessageArgumentIndex(kind);
            if (HasArgument(invocation, messageArgumentIndex))
            {
                if (!IsSupportedArgument(invocation, messageArgumentIndex))
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetValueArgumentIndex(RuleKind kind)
        {
            if (RequiresValueArgument(kind))
            {
                return 1;
            }

            return -1;
        }

        private static int GetMessageArgumentIndex(RuleKind kind)
        {
            if (RequiresValueArgument(kind))
            {
                return 2;
            }

            return 1;
        }

        private static bool HasArgument(InvocationExpressionSyntax invocation, int argumentIndex)
        {
            return invocation.ArgumentList.Arguments.Count > argumentIndex;
        }

        private static bool IsSupportedArgument(InvocationExpressionSyntax invocation, int argumentIndex)
        {
            if (!HasArgument(invocation, argumentIndex))
            {
                return false;
            }

            return invocation.ArgumentList.Arguments[argumentIndex].Expression is LiteralExpressionSyntax;
        }

        private static bool IsSupportedRequirementMethod(
            SemanticModel semanticModel,
            ExpressionSyntax expression,
            out string requirementMethod)
        {
            requirementMethod = string.Empty;

            var symbolInfo = semanticModel.GetSymbolInfo(expression);
            var symbol = symbolInfo.Symbol ?? GetSingleCandidate(symbolInfo);
            if (!(symbol is IMethodSymbol method))
            {
                return false;
            }

            if (!method.IsStatic)
            {
                return false;
            }

            if (method.TypeArguments.Length != 0)
            {
                return false;
            }

            if (method.Parameters.Length != 1)
            {
                return false;
            }

            if (method.ReturnType.SpecialType != SpecialType.System_Boolean)
            {
                return false;
            }

            requirementMethod = GetRequirementMethodName(method);
            return true;
        }

        private static ISymbol? GetSingleCandidate(SymbolInfo symbolInfo)
        {
            if (symbolInfo.CandidateSymbols.Length != 1)
            {
                return null;
            }

            return symbolInfo.CandidateSymbols[0];
        }

        private static string GetRequirementMethodName(IMethodSymbol method)
        {
            var containingType = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return containingType + "." + method.Name;
        }

        private static bool RequiresValueArgument(RuleKind kind)
        {
            if (kind == RuleKind.TextLengthAtLeast)
            {
                return true;
            }

            if (kind == RuleKind.TextLengthAtMost)
            {
                return true;
            }

            if (kind == RuleKind.Above)
            {
                return true;
            }

            if (kind == RuleKind.AtLeast)
            {
                return true;
            }

            if (kind == RuleKind.Below)
            {
                return true;
            }

            if (kind == RuleKind.AtMost)
            {
                return true;
            }

            return kind == RuleKind.Matches;
        }

        private static bool IsValidCustomRule(ITypeSymbol? typeSymbol, INamedTypeSymbol commandType)
        {
            if (!(typeSymbol is INamedTypeSymbol namedType))
            {
                return false;
            }

            foreach (var candidate in namedType.AllInterfaces)
            {
                if (IsAsyncValidationRule(candidate, commandType))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAsyncValidationRule(INamedTypeSymbol candidate, INamedTypeSymbol commandType)
        {
            if (candidate.ContainingNamespace.ToDisplayString() != "TinyValidations")
            {
                return false;
            }

            if (candidate.Name != "IAsyncValidationRule")
            {
                return false;
            }

            if (candidate.TypeArguments.Length != 1)
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(candidate.TypeArguments[0], commandType);
        }
    }
}
