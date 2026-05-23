using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Emission.Writing;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal sealed class EmailRuleEmitter : IRuleEmitter
    {
        public bool CanEmit(RuleDefinition rule) => rule.Kind == RuleKind.Email;

        public void Emit(RuleDefinition rule, SourceWriter writer)
        {
            var message = RuleMessage.For(rule, rule.MemberPath + " must be a valid email address.");
            writer.WriteLine("if (!string.IsNullOrWhiteSpace(" + rule.MemberAccess + "))");
            writer.OpenBlock();
            writer.WriteLine("if (!global::TinyValidations.TinyEmailAddress.IsValid(" + rule.MemberAccess + "))");
            writer.OpenBlock();
            writer.WriteLine("errors.Add(" + StringLiteral.Create(rule.MemberPath) + ", " + message + ");");
            writer.CloseBlock();
            writer.CloseBlock();
        }
    }
}

