using System;

namespace NetToolBox.Search.AzureCognitiveSearch
{
    internal sealed class AzureSearchSettings
    {
        public Uri Endpoint { get; set; } = null!;

        public string Key { get; set; } = null!;
    }
}