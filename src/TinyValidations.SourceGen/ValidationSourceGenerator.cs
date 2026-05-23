using Microsoft.CodeAnalysis;
using TinyValidations.SourceGen.Generation;

namespace TinyValidations.SourceGen
{
    [Generator]
    public sealed class ValidationSourceGenerator : IIncrementalGenerator
    {
        private readonly SourceGenerationRegistration _registration = new SourceGenerationRegistration();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _registration.Register(context);
        }
    }
}
