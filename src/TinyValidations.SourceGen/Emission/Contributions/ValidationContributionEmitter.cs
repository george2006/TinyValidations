using TinyValidations.SourceGen.Emission.Writing;
using TinyValidations.SourceGen.Planning;

namespace TinyValidations.SourceGen.Emission.Contributions
{
    internal sealed class ValidationContributionEmitter
    {
        public void Emit(GeneratedValidationPlan plan, SourceWriter writer)
        {
            writer.WriteLine("internal sealed class TinyGeneratedValidationContribution : global::TinyValidations.ITinyValidationContribution");
            writer.OpenBlock();
            writer.WriteLine("public void Register(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
            writer.OpenBlock();

            foreach (var runner in plan.Runners)
            {
                writer.WriteLine("services.TryAddEnumerable(global::Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Scoped<global::TinyValidations.ITinyValidationRunner<" + runner.CommandTypeName + ">, " + runner.RunnerName + ">());");

                foreach (var customRuleType in runner.CustomRuleTypes)
                {
                    writer.WriteLine("services.TryAddScoped<" + customRuleType + ">();");
                }
            }

            writer.CloseBlock();
            writer.CloseBlock();
        }
    }
}
