using TinyValidations.SourceGen.Emission.Writing;
using TinyValidations.SourceGen.Planning;

namespace TinyValidations.SourceGen.Emission.Contributions
{
    internal sealed class ValidationContributionEmitter
    {
        public void Emit(GeneratedValidationPlan plan, SourceWriter writer)
        {
            writer.WriteLine("internal sealed class TinyGeneratedValidationContribution : global::TinyValidations.ITinyValidationContribution, global::TinyValidations.ITinyValidationStructureContribution");
            writer.OpenBlock();
            EmitValidations(plan, writer);
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

        private static void EmitValidations(GeneratedValidationPlan plan, SourceWriter writer)
        {
            writer.WriteLine("public global::System.Collections.Generic.IReadOnlyList<global::TinyValidations.TinyValidationStructure> Validations");
            writer.OpenBlock();
            writer.WriteLine("get");
            writer.OpenBlock();
            writer.WriteLine("return new global::TinyValidations.TinyValidationStructure[]");
            writer.OpenBlock();

            foreach (var runner in plan.Runners)
            {
                writer.WriteLine("new global::TinyValidations.TinyValidationStructure(typeof(" + runner.CommandTypeName + "), typeof(" + runner.ValidationTypeName + "), new global::TinyValidations.TinyValidationRuleStructure[] { " + GetBuiltInRules(runner) + " }, new global::System.Type[] { " + GetCustomRuleTypes(runner) + " }),");
            }

            writer.CloseBlock();
            writer.WriteLine(";");
            writer.CloseBlock();
            writer.CloseBlock();
        }

        private static string GetBuiltInRules(GeneratedRunnerPlan runner)
        {
            var rules = new System.Collections.Generic.List<string>();

            foreach (var rule in runner.Rules)
            {
                if (rule.Kind != TinyValidations.SourceGen.Model.RuleKind.Use)
                {
                    rules.Add(
                        "new global::TinyValidations.TinyValidationRuleStructure(" +
                        StringLiteral.Create(rule.MemberPath) + ", " +
                        StringLiteral.Create(rule.Kind.ToString()) + ")");
                }
            }

            return string.Join(", ", rules);
        }

        private static string GetCustomRuleTypes(GeneratedRunnerPlan runner)
        {
            return string.Join(", ", System.Linq.Enumerable.Select(
                runner.CustomRuleTypes,
                type => "typeof(" + type + ")"));
        }
    }
}
