using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Emission.Writing;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal sealed class ComparableRuleEmitter : IRuleEmitter
    {
        public bool CanEmit(RuleDefinition rule)
        {
            if (rule.Kind == RuleKind.Above)
            {
                return true;
            }

            if (rule.Kind == RuleKind.AtLeast)
            {
                return true;
            }

            if (rule.Kind == RuleKind.Below)
            {
                return true;
            }

            return rule.Kind == RuleKind.AtMost;
        }

        public void Emit(RuleDefinition rule, SourceWriter writer)
        {
            var comparison = GetComparison(rule.Kind);
            var text = GetMessageText(rule);
            var message = RuleMessage.For(rule, text);
            var compare = CreateCompareExpression(rule);

            writer.WriteLine("if (" + compare + " " + comparison + " 0)");
            writer.OpenBlock();
            writer.WriteLine("errors.Add(" + StringLiteral.Create(rule.MemberPath) + ", " + message + ");");
            writer.CloseBlock();
        }

        private static string CreateCompareExpression(RuleDefinition rule)
        {
            return "global::System.Collections.Generic.Comparer<" + rule.ComparisonTypeName + ">.Default.Compare(" + rule.MemberAccess + ", " + rule.Argument + ")";
        }

        private static string GetComparison(RuleKind kind)
        {
            if (kind == RuleKind.Above)
            {
                return "<=";
            }

            if (kind == RuleKind.AtLeast)
            {
                return "<";
            }

            if (kind == RuleKind.Below)
            {
                return ">=";
            }

            return ">";
        }

        private static string GetMessageText(RuleDefinition rule)
        {
            if (rule.Kind == RuleKind.Above)
            {
                return rule.MemberPath + " must be above " + rule.ArgumentDisplay + ".";
            }

            if (rule.Kind == RuleKind.AtLeast)
            {
                return rule.MemberPath + " must be at least " + rule.ArgumentDisplay + ".";
            }

            if (rule.Kind == RuleKind.Below)
            {
                return rule.MemberPath + " must be below " + rule.ArgumentDisplay + ".";
            }

            return rule.MemberPath + " must be at most " + rule.ArgumentDisplay + ".";
        }
    }
}

