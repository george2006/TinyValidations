using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Emission.Writing;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal sealed class MatchesRuleEmitter : IRuleEmitter
    {
        public bool CanEmit(RuleDefinition rule) => rule.Kind == RuleKind.Matches;

        public void Emit(RuleDefinition rule, SourceWriter writer)
        {
            var message = RuleMessage.For(rule, rule.MemberPath + " has an invalid format.");
            writer.WriteLine("if (!string.IsNullOrWhiteSpace(" + rule.MemberAccess + "))");
            writer.OpenBlock();
            writer.WriteLine("if (!global::System.Text.RegularExpressions.Regex.IsMatch(" + rule.MemberAccess + ", " + rule.Argument + "))");
            writer.OpenBlock();
            writer.WriteLine("errors.Add(" + StringLiteral.Create(rule.MemberPath) + ", " + message + ");");
            writer.CloseBlock();
            writer.CloseBlock();
        }
    }
}

