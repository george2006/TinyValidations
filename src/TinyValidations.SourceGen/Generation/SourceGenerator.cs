using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using TinyValidations.SourceGen.Analysis.Declarations;
using TinyValidations.SourceGen.Emission.Source;
using TinyValidations.SourceGen.Planning;
using TinyValidations.SourceGen.Validation;

namespace TinyValidations.SourceGen.Generation
{
    internal sealed class SourceGenerator
    {
        private readonly ValidationDeclarationAnalyzer _analysis = new ValidationDeclarationAnalyzer();
        private readonly ValidationDefinitionValidator _validation = new ValidationDefinitionValidator();
        private readonly ValidationPlanBuilder _planning = new ValidationPlanBuilder();
        private readonly GeneratedSourceEmitter _emission = new GeneratedSourceEmitter();

        public string? Generate(
            Compilation compilation,
            ImmutableArray<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax> candidates,
            System.Action<Diagnostic> reportDiagnostic)
        {
            var model = _analysis.Analyze(compilation, candidates);
            var canGenerate = _validation.Validate(model, reportDiagnostic);
            if (!canGenerate)
            {
                return null;
            }

            if (model.Validations.Count == 0)
            {
                return null;
            }

            var plan = _planning.Build(model);
            return _emission.Emit(plan);
        }
    }
}
