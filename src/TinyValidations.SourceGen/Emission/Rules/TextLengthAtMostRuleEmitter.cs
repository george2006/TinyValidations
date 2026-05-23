using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Emission.Writing;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal sealed class TextLengthAtMostRuleEmitter : IRuleEmitter
    {
        public bool CanEmit(RuleDefinition rule) => rule.Kind == RuleKind.TextLengthAtMost;

        public void Emit(RuleDefinition rule, SourceWriter writer)
        {
            var message = RuleMessage.For(rule, rule.MemberPath + " must contain at most " + rule.Argument + " characters.");
            writer.WriteLine("if (" + rule.MemberAccess + " is not null)");
            writer.OpenBlock();
            writer.WriteLine("if (" + rule.MemberAccess + ".Length > " + rule.Argument + ")");
            writer.OpenBlock();
            writer.WriteLine("errors.Add(" + StringLiteral.Create(rule.MemberPath) + ", " + message + ");");
            writer.CloseBlock();
            writer.CloseBlock();
        }
    }
}

