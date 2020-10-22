using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetToolBox.Search.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetToolBox.Search.AzureCognitiveSearch
{
    internal sealed class AzureSearchClient : ISearchClient
    {
        private readonly AzureSearchFactory _factory;
        private readonly ILogger<AzureSearchClient> _logger;
        private readonly IMemoryCache _memoryCache;

        public AzureSearchClient(AzureSearchFactory factory, ILogger<AzureSearchClient> logger, IMemoryCache memoryCache)
        {
            _factory = factory;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task CreateAndPopulateIndexAsync<T>(string indexName, List<T> entries)
        {
            var serviceClient = _factory.SearchIndexClient;
            await DeleteIndexAsync(indexName).ConfigureAwait(false);

            var definition = new SearchIndex(indexName) { Fields = new FieldBuilder().Build(typeof(T)) };
            await serviceClient.CreateIndexAsync(definition).ConfigureAwait(false);

            var indexClient = serviceClient.GetSearchClient(indexName);

            var batch = IndexDocumentsBatch.Create<T>();

            _logger.LogInformation("Indexing {Count} entries for Index {IndexName}", entries.Count, indexName);

            for (var counter = 0; counter < entries.Count; counter++)
            {
                batch.Actions.Add(IndexDocumentsAction.Upload(entries[counter]));
                if (counter % 5000 == 0) //it's too big for one batch
                {
                    await indexClient.IndexDocumentsAsync(batch, new Azure.Search.Documents.IndexDocumentsOptions { ThrowOnAnyError = true }).ConfigureAwait(false);
                    batch = IndexDocumentsBatch.Create<T>();
                    _logger.LogInformation("Submitted {Counter} of {Total} for Index {IndexName}", counter, entries.Count, indexName);
                }
            }

            await indexClient.IndexDocumentsAsync(batch, new Azure.Search.Documents.IndexDocumentsOptions { ThrowOnAnyError = true }).ConfigureAwait(false);
            _logger.LogInformation("Completed index submission for Index {IndexName}", indexName);
        }

        public Task DeleteIndexAsync(string indexName)
        {
            var serviceClient = _factory.SearchIndexClient;
            return serviceClient.DeleteIndexAsync(indexName);
        }

        /// <summary>
        /// Lists Indexes at the configured endpoint
        /// NOTE: This function caches the index entries for one minute since Azure throttles calls to ListIndexes, which causes lots of problems when using listindexes as a proxy for healthchecks that go through Azure Front Door or something similar that checks the health from a lot of endpoints
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> ListIndexesAsync()
        {
            var indexes = await _memoryCache.GetOrCreateAsync<List<string>>("NetToolBox.AzureCognitiveSearch.ListIndexes", async entry =>
             {
                 entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1); //only query indexes every minute due to throttling on listindex
                 var retval = new List<string>();

                 var serviceClient = _factory.SearchIndexClient;
                 var indexes = serviceClient.GetIndexesAsync();

                 await foreach (var index in indexes.ConfigureAwait(false))
                 {
                     retval.Add(index.Name);
                 }

                 return retval.OrderByDescending(x => x).ToList();
             });
            return indexes;
        }

        public async Task<SearchResponse<T>> SearchIndexAsync<T>(string indexName, string searchTerm, string orderBy, int pageSize = 100, int skip = 0)
        {
            var options = new SearchOptions
            {
                IncludeTotalCount = true,
                Size = pageSize,
                Skip = skip,
            };
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                options.OrderBy.Add(orderBy);
            }

            var searchClient = _factory.GetSearchClient(indexName);

            var searchResponse = await searchClient.SearchAsync<T>(searchTerm, options).ConfigureAwait(false);

            var retval = new SearchResponse<T>
            {
                Results = new List<T>(),
                TotalResultCount = searchResponse.Value.TotalCount
            };
            var results = searchResponse.Value.GetResultsAsync();

            await foreach (var result in results.ConfigureAwait(false))
            {
                retval.Results.Add(result.Document);
            }

            retval.NextSkipValue = skip + retval.Results.Count;

            return retval;
        }
    }
}