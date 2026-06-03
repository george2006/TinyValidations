using System.Collections.Generic;
using System.Linq;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Planning
{
    internal sealed class ValidationPlanBuilder
    {
        public GeneratedValidationPlan Build(ValidationDefinitionSet model)
        {
            var runners = new List<GeneratedRunnerPlan>();

            foreach (var validation in model.Validations)
            {
                var rules = DeduplicateCustomRuleInvocations(validation.Rules);
                var customRules = GetCustomRuleTypes(rules);

                runners.Add(new GeneratedRunnerPlan(
                    SafeName.Create(validation.ValidationTypeName) + "Runner",
                    validation.CommandTypeName,
                    rules,
                    customRules));
            }

            return new GeneratedValidationPlan(runners);
        }

        private static string[] GetCustomRuleTypes(IEnumerable<RuleDefinition> rules)
        {
            return rules
                .Where(rule => rule.Kind == RuleKind.Use)
                .Select(rule => rule.CustomRuleType)
                .ToArray();
        }

        private static RuleDefinition[] DeduplicateCustomRuleInvocations(IReadOnlyCollection<RuleDefinition> rules)
        {
            var customRuleTypes = new HashSet<string>();
            var deduplicated = new List<RuleDefinition>();

            foreach (var rule in rules)
            {
                if (ShouldSkipDuplicateCustomRule(rule, customRuleTypes))
                {
                    continue;
                }

                deduplicated.Add(rule);
            }

            return deduplicated.ToArray();
        }

        private static bool ShouldSkipDuplicateCustomRule(
            RuleDefinition rule,
            HashSet<string> customRuleTypes)
        {
            if (rule.Kind != RuleKind.Use)
            {
                return false;
            }

            return !customRuleTypes.Add(rule.CustomRuleType);
        }
    }
}
