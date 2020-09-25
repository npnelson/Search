using System.Collections.Generic;
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
        /// Search a given index and return the results in T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexName">name of the index</param>
        /// <param name="searchTerm">term to search</param>
        /// <param name="pageSize">how many results to return at a time</param>
        /// <param name="skip">used for paging (i.e. to get the second page of 100 results, use skip=100 here </param>
        /// <returns></returns>
        Task<List<T>> SearchIndexAsync<T>(string indexName, string searchTerm, string orderBy, int pageSize = 100, int skip = 0);
    }
}