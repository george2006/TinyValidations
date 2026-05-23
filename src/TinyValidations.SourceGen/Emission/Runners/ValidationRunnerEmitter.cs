using TinyValidations.SourceGen.Emission.Rules;
using TinyValidations.SourceGen.Emission.Writing;
using TinyValidations.SourceGen.Planning;

namespace TinyValidations.SourceGen.Emission.Runners
{
    internal sealed class ValidationRunnerEmitter
    {
        private readonly RuleEmitterSet _ruleEmitters = new RuleEmitterSet();

        public void Emit(GeneratedRunnerPlan runner, SourceWriter writer)
        {
            writer.WriteLine("internal sealed class " + runner.RunnerName + " : global::TinyValidations.ITinyValidationRunner<" + runner.CommandTypeName + ">");
            writer.OpenBlock();
            EmitFields(runner, writer);
            EmitConstructor(runner, writer);
            EmitValidateMethod(runner, writer);
            writer.CloseBlock();
        }

        private static void EmitFields(GeneratedRunnerPlan runner, SourceWriter writer)
        {
            foreach (var customRuleType in runner.CustomRuleTypes)
            {
                writer.WriteLine("private readonly " + customRuleType + " " + CustomRuleFieldName.Create(customRuleType) + ";");
            }

            if (runner.CustomRuleTypes.Count > 0)
            {
                writer.WriteLine();
            }
        }

        private static void EmitConstructor(GeneratedRunnerPlan runner, SourceWriter writer)
        {
            if (runner.CustomRuleTypes.Count == 0)
            {
                return;
            }

            writer.WriteLine("public " + runner.RunnerName + "(");
            for (var index = 0; index < runner.CustomRuleTypes.Count; index++)
            {
                var customRuleType = runner.CustomRuleTypes[index];
                var suffix = GetConstructorParameterSuffix(runner, index);
                writer.WriteLine("    " + customRuleType + " " + ParameterName(customRuleType) + suffix);
            }

            writer.OpenBlock();
            foreach (var customRuleType in runner.CustomRuleTypes)
            {
                writer.WriteLine(CustomRuleFieldName.Create(customRuleType) + " = " + ParameterName(customRuleType) + ";");
            }
            writer.CloseBlock();
            writer.WriteLine();
        }

        private void EmitValidateMethod(GeneratedRunnerPlan runner, SourceWriter writer)
        {
            writer.WriteLine("public async global::System.Threading.Tasks.ValueTask<global::TinyValidations.ValidationResult> ValidateAsync(" + runner.CommandTypeName + " instance, global::System.Threading.CancellationToken cancellationToken)");
            writer.OpenBlock();
            writer.WriteLine("var errors = new global::TinyValidations.ValidationErrorCollection();");
            writer.WriteLine();

            foreach (var rule in runner.Rules)
            {
                _ruleEmitters.Emit(rule, writer);
                writer.WriteLine();
            }

            writer.WriteLine("return errors.ToResult();");
            writer.CloseBlock();
        }

        private static string ParameterName(string typeName)
        {
            var fieldName = CustomRuleFieldName.Create(typeName);
            if (fieldName.Length > 1)
            {
                return fieldName.Substring(1);
            }

            return "rule";
        }

        private static string GetConstructorParameterSuffix(GeneratedRunnerPlan runner, int index)
        {
            if (IsLastConstructorParameter(runner, index))
            {
                return ")";
            }

            return ",";
        }

        private static bool IsLastConstructorParameter(GeneratedRunnerPlan runner, int index)
        {
            return index == runner.CustomRuleTypes.Count - 1;
        }
    }
}
