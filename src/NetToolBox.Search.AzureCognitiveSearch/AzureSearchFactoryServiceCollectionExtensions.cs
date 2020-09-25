using Microsoft.Extensions.Configuration;
using NetToolBox.Search.Abstractions;
using NetToolBox.Search.AzureCognitiveSearch;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureSearchFactoryServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureSearch(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.AddSingleton<AzureSearchFactory>();
            services.AddSingleton<ISearchClient, AzureSearchClient>();
            services.Configure<AzureSearchSettings>(configurationSection);
            return services;
        }
    }
}