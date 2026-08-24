using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Emission.Writing;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal sealed class RequiredRuleEmitter : IRuleEmitter
    {
        public bool CanEmit(RuleDefinition rule) => rule.Kind == RuleKind.Required;

        public void Emit(RuleDefinition rule, SourceWriter writer)
        {
            var message = RuleMessage.For(rule, rule.MemberPath + " is required.");
            writer.WriteLine("if (global::TinyValidations.TinyRequiredValue.IsMissing(" + rule.MemberAccess + "))");
            writer.OpenBlock();
            writer.WriteLine("errors.Add(" + StringLiteral.Create(rule.MemberPath) + ", " + message + ");");
            writer.CloseBlock();
        }
    }
}

