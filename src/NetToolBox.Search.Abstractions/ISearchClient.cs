using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetToolBox.Search.Abstractions
{
    public interface ISearchClient
    {
        /// <summary>
        /// Delete index if it exists
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        Task DeleteIndexAsync(string indexName);

        /// <summary>
        /// Create an index
        /// </summary>
        /// <typeparam name="T">A type indicating the index structure, use annotations from https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/search/Azure.Search.Documents/src/Indexes (i.e. SearchableField, SimpleField) to annotate</typeparam>
        /// <param name="indexName"></param>
        /// <param name="entries"></param>
        /// <returns></returns>
        Task CreateAndPopulateIndexAsync<T>(string indexName, List<T> entries);

        /// <summary>
        /// List existing indexes
        /// </summary>
        /// <returns></returns>
        Task<List<string>> ListIndexesAsync();

        /// <summary>
        /// Search an index
        /// </summary>
        /// <typeparam name="T">The type you want the search results to hydrate</typeparam>
        /// <param name="indexName">name of index</param>
        /// <param name="searchTerm">term to search</param>
        /// <param name="orderBy">CASE SENSITIVE  - the name of one field you would like to order by, to sort descending use sortfield desc</param>
        /// <param name="pageSize">the maximum number of results to return</param>
        /// <param name="skip">the number of entries to skip from the top - useful for paging scenarioes</param>
        /// <param name="cancellationToken">optional cancellationToken</param>
        /// <returns></returns>
        Task<SearchResponse<T>> SearchIndexAsync<T>(string indexName, string searchTerm, string orderBy, int pageSize = 100, int skip = 0, CancellationToken cancellationToken = default);
    }
}