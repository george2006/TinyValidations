using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TinyValidations.SourceGen.Discovery;

namespace TinyValidations.SourceGen.Generation
{
    internal sealed class SourceGenerationRegistration
    {
        private readonly ValidationCandidateProvider _candidates = new ValidationCandidateProvider();
        private readonly SourceGenerator _generator = new SourceGenerator();

        public void Register(IncrementalGeneratorInitializationContext context)
        {
            var declarationCandidates = _candidates.GetCandidates(context);
            var generationInput = context.CompilationProvider.Combine(declarationCandidates);

            context.RegisterSourceOutput(generationInput, (sourceContext, input) =>
            {
                var source = _generator.Generate(input.Left, input.Right, sourceContext.ReportDiagnostic);
                if (source == null)
                {
                    return;
                }

                sourceContext.AddSource("TinyValidations.g.cs", SourceText.From(source, Encoding.UTF8));
            });
        }
    }
}
