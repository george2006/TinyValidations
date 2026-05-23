using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Emission.Writing;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal sealed class NotNullRuleEmitter : IRuleEmitter
    {
        public bool CanEmit(RuleDefinition rule) => rule.Kind == RuleKind.NotNull;

        public void Emit(RuleDefinition rule, SourceWriter writer)
        {
            var message = RuleMessage.For(rule, rule.MemberPath + " must not be null.");
            writer.WriteLine("if (" + rule.MemberAccess + " is null)");
            writer.OpenBlock();
            writer.WriteLine("errors.Add(" + StringLiteral.Create(rule.MemberPath) + ", " + message + ");");
            writer.CloseBlock();
        }
    }
}

