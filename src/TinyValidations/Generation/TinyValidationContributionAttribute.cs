using System;

namespace TinyValidations;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class TinyValidationContributionAttribute : Attribute
{
    public TinyValidationContributionAttribute(Type commandType, Type runnerType)
    {
        CommandType = commandType;
        RunnerType = runnerType;
    }

    public Type CommandType { get; }

    public Type RunnerType { get; }
}
