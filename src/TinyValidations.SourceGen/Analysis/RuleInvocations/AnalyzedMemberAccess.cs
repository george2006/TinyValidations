namespace TinyValidations.SourceGen.Analysis.RuleInvocations
{
    internal sealed class AnalyzedMemberAccess
    {
        public AnalyzedMemberAccess(string path, string access, bool isNullSafe)
        {
            Path = path;
            Access = access;
            IsNullSafe = isNullSafe;
        }

        public string Path { get; }

        public string Access { get; }

        public bool IsNullSafe { get; }
    }
}
