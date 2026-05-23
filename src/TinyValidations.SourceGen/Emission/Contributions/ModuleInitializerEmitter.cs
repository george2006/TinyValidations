using TinyValidations.SourceGen.Emission.Writing;

namespace TinyValidations.SourceGen.Emission.Contributions
{
    internal sealed class ModuleInitializerEmitter
    {
        public void Emit(SourceWriter writer)
        {
            writer.WriteLine("internal static class TinyGeneratedValidationModuleInitializer");
            writer.OpenBlock();
            writer.WriteLine("[global::System.Runtime.CompilerServices.ModuleInitializer]");
            writer.WriteLine("internal static void Initialize()");
            writer.OpenBlock();
            writer.WriteLine("global::TinyValidations.TinyValidationBootstrap.AddContribution(new TinyGeneratedValidationContribution());");
            writer.CloseBlock();
            writer.CloseBlock();
        }
    }
}
