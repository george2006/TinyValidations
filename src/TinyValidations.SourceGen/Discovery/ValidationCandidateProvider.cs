using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TinyValidations.SourceGen.Discovery
{
    internal sealed class ValidationCandidateProvider
    {
        public IncrementalValueProvider<ImmutableArray<ClassDeclarationSyntax>> GetCandidates(
            IncrementalGeneratorInitializationContext context)
        {
            return context.SyntaxProvider
                .CreateSyntaxProvider(IsPossibleValidationDeclaration, GetValidationCandidate)
                .Where(static candidate => candidate != null)
                .Select(static (candidate, cancellationToken) => GetRequiredCandidate(candidate))
                .Collect();
        }

        private static bool IsPossibleValidationDeclaration(SyntaxNode node, CancellationToken cancellationToken)
        {
            if (!(node is ClassDeclarationSyntax declaration))
            {
                return false;
            }

            if (!HasBaseList(declaration))
            {
                return false;
            }

            return LooksLikeValidationDeclaration(declaration);
        }

        private static ClassDeclarationSyntax? GetValidationCandidate(
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken)
        {
            var declaration = (ClassDeclarationSyntax)context.Node;
            var validationType = context.SemanticModel.GetDeclaredSymbol(declaration, cancellationToken);

            if (!(validationType is INamedTypeSymbol namedType))
            {
                return null;
            }

            foreach (var candidate in namedType.AllInterfaces)
            {
                if (IsValidationInterface(candidate))
                {
                    return declaration;
                }
            }

            return null;
        }

        private static ClassDeclarationSyntax GetRequiredCandidate(ClassDeclarationSyntax? candidate)
        {
            if (candidate == null)
            {
                throw new InvalidOperationException("Validation candidate was not available.");
            }

            return candidate;
        }

        private static bool HasBaseList(ClassDeclarationSyntax declaration)
        {
            return declaration.BaseList != null;
        }

        private static bool LooksLikeValidationDeclaration(ClassDeclarationSyntax declaration)
        {
            if (HasValidationInterfaceName(declaration))
            {
                return true;
            }

            return HasDefineMethod(declaration);
        }

        private static bool HasValidationInterfaceName(ClassDeclarationSyntax declaration)
        {
            var baseList = declaration.BaseList;
            if (baseList == null)
            {
                return false;
            }

            foreach (var baseType in baseList.Types)
            {
                if (IsValidationInterfaceName(baseType.Type))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsValidationInterfaceName(TypeSyntax type)
        {
            if (type is GenericNameSyntax genericName)
            {
                return genericName.Identifier.ValueText == "IValidation";
            }

            if (type is QualifiedNameSyntax qualifiedName)
            {
                return IsValidationInterfaceName(qualifiedName.Right);
            }

            if (type is AliasQualifiedNameSyntax aliasQualifiedName)
            {
                return IsValidationInterfaceName(aliasQualifiedName.Name);
            }

            return false;
        }

        private static bool HasDefineMethod(ClassDeclarationSyntax declaration)
        {
            foreach (var member in declaration.Members)
            {
                if (!(member is MethodDeclarationSyntax method))
                {
                    continue;
                }

                if (IsDefineMethod(method))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDefineMethod(MethodDeclarationSyntax method)
        {
            return method.Identifier.ValueText == "Define";
        }

        private static bool IsValidationInterface(INamedTypeSymbol candidate)
        {
            if (candidate.ContainingNamespace.ToDisplayString() != "TinyValidations")
            {
                return false;
            }

            if (candidate.Name != "IValidation")
            {
                return false;
            }

            return candidate.TypeArguments.Length == 1;
        }
    }
}
