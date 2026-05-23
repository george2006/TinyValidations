using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Emission.Writing;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal sealed class CustomRuleEmitter : IRuleEmitter
    {
        public bool CanEmit(RuleDefinition rule) => rule.Kind == RuleKind.Use;

        public void Emit(RuleDefinition rule, SourceWriter writer)
        {
            var fieldName = CustomRuleFieldName.Create(rule.CustomRuleType);
            writer.WriteLine("await " + fieldName + ".ValidateAsync(instance, errors, cancellationToken).ConfigureAwait(false);");
        }
    }
}

