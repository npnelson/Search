namespace NetToolBox.Search.TestHarness
{
    public sealed class SnomedSearchResult
    {
        public string Id { get; set; } = null!;

        public string PreferredTerm { get; set; } = null!;

        public string Icd10Code { get; set; } = null!;
    }
}