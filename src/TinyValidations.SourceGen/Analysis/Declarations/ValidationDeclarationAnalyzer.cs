using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Validation;

namespace TinyValidations.SourceGen.Analysis.Declarations
{
    internal sealed class ValidationDeclarationAnalyzer
    {
        private readonly DefineMethodAnalyzer _defineMethodAnalyzer = new DefineMethodAnalyzer();

        public ValidationDefinitionSet Analyze(
            Compilation compilation,
            IEnumerable<ClassDeclarationSyntax> candidates)
        {
            var validations = new List<ValidationDefinition>();
            var issues = new List<ValidationIssue>();
            var symbols = LoadSymbols(compilation);

            if (symbols == null)
            {
                return new ValidationDefinitionSet(validations, issues);
            }

            AnalyzeCandidates(compilation, candidates, symbols, validations, issues);

            return new ValidationDefinitionSet(validations, issues);
        }

        private void AnalyzeCandidates(
            Compilation compilation,
            IEnumerable<ClassDeclarationSyntax> candidates,
            ValidationSymbols symbols,
            List<ValidationDefinition> validations,
            List<ValidationIssue> issues)
        {
            foreach (var candidate in candidates)
            {
                AnalyzeCandidate(compilation, candidate, symbols, validations, issues);
            }
        }

        private void AnalyzeCandidate(
            Compilation compilation,
            ClassDeclarationSyntax candidate,
            ValidationSymbols symbols,
            List<ValidationDefinition> validations,
            List<ValidationIssue> issues)
        {
            var target = LoadAnalysisTarget(compilation, candidate, symbols);
            if (target == null)
            {
                return;
            }

            var semanticModel = target.SemanticModel;
            var validationType = target.ValidationType;
            var commandType = target.CommandType;

            var definition = AnalyzeDefineMethod(
                semanticModel,
                candidate,
                validationType,
                commandType,
                symbols.ValidationRules,
                issues);

            if (definition == null)
            {
                AddWrongDefineSignatureIssue(candidate, validationType, issues);
                return;
            }

            validations.Add(definition);
        }

        private static ValidationAnalysisTarget? LoadAnalysisTarget(
            Compilation compilation,
            ClassDeclarationSyntax candidate,
            ValidationSymbols symbols)
        {
            var semanticModel = compilation.GetSemanticModel(candidate.SyntaxTree);
            var validationType = GetValidationType(semanticModel, candidate);
            if (validationType == null)
            {
                return null;
            }

            var commandType = TryGetCommandType(validationType, symbols.ValidationInterface);

            if (commandType == null)
            {
                return null;
            }

            return new ValidationAnalysisTarget(semanticModel, validationType, commandType);
        }

        private ValidationDefinition? AnalyzeDefineMethod(
            SemanticModel semanticModel,
            ClassDeclarationSyntax candidate,
            INamedTypeSymbol validationType,
            INamedTypeSymbol commandType,
            INamedTypeSymbol validationRules,
            List<ValidationIssue> issues)
        {
            return _defineMethodAnalyzer.Analyze(
                semanticModel,
                candidate,
                validationType,
                commandType,
                validationRules,
                issues);
        }

        private static INamedTypeSymbol? GetValidationType(
            SemanticModel semanticModel,
            ClassDeclarationSyntax candidate)
        {
            return semanticModel.GetDeclaredSymbol(candidate) as INamedTypeSymbol;
        }

        private static ValidationSymbols? LoadSymbols(Compilation compilation)
        {
            var validationInterface = compilation.GetTypeByMetadataName("TinyValidations.IValidation`1");
            if (validationInterface == null)
            {
                return null;
            }

            var validationRules = compilation.GetTypeByMetadataName("TinyValidations.ValidationRules`1");
            if (validationRules == null)
            {
                return null;
            }

            return new ValidationSymbols(validationInterface, validationRules);
        }

        private static INamedTypeSymbol? TryGetCommandType(
            INamedTypeSymbol validationType,
            INamedTypeSymbol validationInterface)
        {
            foreach (var candidate in validationType.AllInterfaces)
            {
                if (IsValidationInterface(candidate, validationInterface))
                {
                    return candidate.TypeArguments[0] as INamedTypeSymbol;
                }
            }

            return null;
        }

        private static bool IsValidationInterface(
            INamedTypeSymbol candidate,
            INamedTypeSymbol validationInterface)
        {
            if (!HasSingleTypeArgument(candidate))
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, validationInterface);
        }

        private static bool HasSingleTypeArgument(INamedTypeSymbol candidate)
        {
            return candidate.TypeArguments.Length == 1;
        }

        private static void AddWrongDefineSignatureIssue(
            ClassDeclarationSyntax candidate,
            INamedTypeSymbol validationType,
            List<ValidationIssue> issues)
        {
            issues.Add(new ValidationIssue(
                ValidationDiagnostics.WrongDefineSignature,
                candidate.Identifier.GetLocation(),
                validationType.Name));
        }

        private sealed class ValidationSymbols
        {
            public ValidationSymbols(
                INamedTypeSymbol validationInterface,
                INamedTypeSymbol validationRules)
            {
                ValidationInterface = validationInterface;
                ValidationRules = validationRules;
            }

            public INamedTypeSymbol ValidationInterface { get; }

            public INamedTypeSymbol ValidationRules { get; }
        }

        private sealed class ValidationAnalysisTarget
        {
            public ValidationAnalysisTarget(
                SemanticModel semanticModel,
                INamedTypeSymbol validationType,
                INamedTypeSymbol commandType)
            {
                SemanticModel = semanticModel;
                ValidationType = validationType;
                CommandType = commandType;
            }

            public SemanticModel SemanticModel { get; }

            public INamedTypeSymbol ValidationType { get; }

            public INamedTypeSymbol CommandType { get; }
        }
    }
}
