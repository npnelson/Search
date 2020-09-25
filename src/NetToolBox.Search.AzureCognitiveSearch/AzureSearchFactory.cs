using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace NetToolBox.Search.AzureCognitiveSearch
{
    internal sealed class AzureSearchFactory
    {
        private readonly ConcurrentDictionary<string, SearchClient> _searchClientDictionary = new ConcurrentDictionary<string, SearchClient>();
        private readonly IOptionsMonitor<AzureSearchSettings> _optionsMonitor;

        public SearchIndexClient SearchIndexClient { get; private set; }
        private Uri _lastEndpointUsed = null!;

        public AzureSearchFactory(IOptionsMonitor<AzureSearchSettings> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
            var opts = _optionsMonitor.CurrentValue;
            SearchIndexClient = new SearchIndexClient(opts.Endpoint, new AzureKeyCredential(opts.Key));
        }

        public SearchClient GetSearchClient(string indexName)
        {
            var opts = _optionsMonitor.CurrentValue;

            if (opts.Endpoint != _lastEndpointUsed)
            {
                _searchClientDictionary.Clear();
                _lastEndpointUsed = opts.Endpoint;
                SearchIndexClient = new SearchIndexClient(opts.Endpoint, new AzureKeyCredential(opts.Key));
            }

            if (!_searchClientDictionary.ContainsKey(indexName))
            {
                var searchClient = SearchIndexClient.GetSearchClient(indexName);

                _searchClientDictionary.TryAdd(indexName, searchClient);
            }

            return _searchClientDictionary[indexName]; //I think we have a chance for a rare race condition where the endpoint changes and one thread clears to make the new endpoints but another thread adds one to the old endpoint after the clear, but not worth addressing that edge case for now
        }
    }
}