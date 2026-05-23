using TinyValidations.SourceGen.Emission.Writing;
using TinyValidations.SourceGen.Planning;

namespace TinyValidations.SourceGen.Emission.Contributions
{
    internal sealed class ValidationContributionMetadataEmitter
    {
        public void Emit(GeneratedValidationPlan plan, SourceWriter writer)
        {
            foreach (var runner in plan.Runners)
            {
                writer.WriteLine("[assembly: global::TinyValidations.TinyValidationContributionAttribute(typeof(" + runner.CommandTypeName + "), typeof(global::TinyValidations.Generated." + runner.RunnerName + "))]");
            }
        }
    }
}
