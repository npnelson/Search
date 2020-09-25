using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetToolBox.Search.Abstractions;
using System;
using System.Threading.Tasks;

namespace NetToolBox.Search.TestHarness
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddAzureSearch(Configuration.GetSection("AzureSearchSettings"));
            serviceCollection.AddLogging(logging => logging.AddConsole());
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var searchClient = serviceProvider.GetRequiredService<ISearchClient>();

            var results = await searchClient.SearchIndexAsync<SnomedSearchResult>("snomed", "heart", "Usage desc");

            foreach (var result in results)
            {
                Console.WriteLine(result.PreferredTerm);
            }

            Console.ReadLine();
        }
    }
}