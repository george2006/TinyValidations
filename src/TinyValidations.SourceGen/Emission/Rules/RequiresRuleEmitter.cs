using TinyValidations.SourceGen.Emission.Writing;
using TinyValidations.SourceGen.Model;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal sealed class RequiresRuleEmitter : IRuleEmitter
    {
        public bool CanEmit(RuleDefinition rule) => rule.Kind == RuleKind.Requires;

        public void Emit(RuleDefinition rule, SourceWriter writer)
        {
            writer.WriteLine("if (!" + rule.RequirementMethod + "(" + rule.MemberAccess + "))");
            writer.OpenBlock();
            writer.WriteLine("errors.Add(" + StringLiteral.Create(rule.MemberPath) + ", " + rule.Message + ");");
            writer.CloseBlock();
        }
    }
}
