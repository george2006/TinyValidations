using System.Collections.Generic;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Planning
{
    internal sealed class GeneratedRunnerPlan
    {
        public GeneratedRunnerPlan(string runnerName, string validationTypeName, string commandTypeName, IReadOnlyList<RuleDefinition> rules, IReadOnlyList<string> customRuleTypes)
        {
            RunnerName = runnerName;
            ValidationTypeName = validationTypeName;
            CommandTypeName = commandTypeName;
            Rules = rules;
            CustomRuleTypes = customRuleTypes;
        }

        public string RunnerName { get; }

        public string ValidationTypeName { get; }

        public string CommandTypeName { get; }

        public IReadOnlyList<RuleDefinition> Rules { get; }

        public IReadOnlyList<string> CustomRuleTypes { get; }
    }
}
