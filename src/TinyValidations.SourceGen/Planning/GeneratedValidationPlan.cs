using System.Collections.Generic;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Planning
{
    internal sealed class GeneratedValidationPlan
    {
        public GeneratedValidationPlan(IReadOnlyList<GeneratedRunnerPlan> runners)
        {
            Runners = runners;
        }

        public IReadOnlyList<GeneratedRunnerPlan> Runners { get; }
    }
}
