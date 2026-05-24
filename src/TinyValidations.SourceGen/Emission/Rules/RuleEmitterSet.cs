using System.Collections.Generic;
using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Emission.Writing;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal sealed class RuleEmitterSet
    {
        private readonly IReadOnlyList<IRuleEmitter> _emitters = new IRuleEmitter[]
        {
            new RequiredRuleEmitter(),
            new HasTextRuleEmitter(),
            new NotNullRuleEmitter(),
            new HasItemsRuleEmitter(),
            new EmailRuleEmitter(),
            new TextLengthAtLeastRuleEmitter(),
            new TextLengthAtMostRuleEmitter(),
            new ComparableRuleEmitter(),
            new MatchesRuleEmitter(),
            new RequiresRuleEmitter(),
            new CustomRuleEmitter()
        };

        public void Emit(RuleDefinition rule, SourceWriter writer)
        {
            foreach (var emitter in _emitters)
            {
                if (emitter.CanEmit(rule))
                {
                    emitter.Emit(rule, writer);
                    return;
                }
            }
        }
    }
}

