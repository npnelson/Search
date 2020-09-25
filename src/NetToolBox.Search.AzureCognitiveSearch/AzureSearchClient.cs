using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using NetToolBox.Search.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetToolBox.Search.AzureCognitiveSearch
{
    internal sealed class AzureSearchClient : ISearchClient
    {
        private readonly AzureSearchFactory _factory;
        private readonly ILogger<AzureSearchClient> _logger;

        public AzureSearchClient(AzureSearchFactory factory, ILogger<AzureSearchClient> logger)
        {
            _factory = factory;
            _logger = logger;
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

        public async Task<List<string>> ListIndexesAsync()
        {
            var retval = new List<string>();

            var serviceClient = _factory.SearchIndexClient;
            var indexes = serviceClient.GetIndexesAsync();

            await foreach (var index in indexes.ConfigureAwait(false))
            {
                retval.Add(index.Name);
            }

            return retval;
        }

        //TODO: modify result to accomodate paging/total results
        public async Task<List<T>> SearchIndexAsync<T>(string indexName, string searchTerm, string orderBy, int pageSize = 100, int skip = 0)
        {
            var retval = new List<T>();
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

            var results = searchResponse.Value.GetResultsAsync();

            await foreach (var result in results.ConfigureAwait(false))
            {
                retval.Add(result.Document);
            }

            return retval;
        }
    }
}