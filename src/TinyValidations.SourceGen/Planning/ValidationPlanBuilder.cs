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
                var customRules = validation.Rules
                    .Where(rule => rule.Kind == RuleKind.Use)
                    .Select(rule => rule.CustomRuleType)
                    .Distinct()
                    .ToArray();

                runners.Add(new GeneratedRunnerPlan(
                    SafeName.Create(validation.ValidationTypeName) + "Runner",
                    validation.CommandTypeName,
                    validation.Rules,
                    customRules));
            }

            return new GeneratedValidationPlan(runners);
        }
    }
}
