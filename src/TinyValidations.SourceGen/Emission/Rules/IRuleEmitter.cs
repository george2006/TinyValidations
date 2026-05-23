using TinyValidations.SourceGen.Model;
using TinyValidations.SourceGen.Emission.Writing;

namespace TinyValidations.SourceGen.Emission.Rules
{
    internal interface IRuleEmitter
    {
        bool CanEmit(RuleDefinition rule);

        void Emit(RuleDefinition rule, SourceWriter writer);
    }
}

