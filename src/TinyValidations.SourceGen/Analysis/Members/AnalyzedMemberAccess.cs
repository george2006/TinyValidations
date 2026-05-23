namespace TinyValidations.SourceGen.Analysis.Members
{
    internal sealed class AnalyzedMemberAccess
    {
        public AnalyzedMemberAccess(string path, string access)
        {
            Path = path;
            Access = access;
        }

        public string Path { get; }

        public string Access { get; }
    }
}
