using System.Collections.Generic;

namespace NetToolBox.Search.Abstractions
{
    public sealed class SearchResponse<T>
    {
        public long? ResultCount { get; set; }

        public List<T> Results { get; set; } = null!;

        public int NextSkipValue { get; set; }
    }
}