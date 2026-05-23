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
            writer.WriteLine("if (" + rule.MemberAccess + " is null)");
            writer.OpenBlock();
            writer.WriteLine("errors.Add(" + StringLiteral.Create(rule.MemberPath) + ", " + message + ");");
            writer.CloseBlock();
            writer.WriteLine("else if (" + rule.MemberAccess + " is string __text)");
            writer.OpenBlock();
            writer.WriteLine("if (string.IsNullOrWhiteSpace(__text))");
            writer.OpenBlock();
            writer.WriteLine("errors.Add(" + StringLiteral.Create(rule.MemberPath) + ", " + message + ");");
            writer.CloseBlock();
            writer.CloseBlock();
        }
    }
}

