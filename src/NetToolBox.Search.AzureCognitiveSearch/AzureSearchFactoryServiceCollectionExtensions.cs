using Microsoft.Extensions.Configuration;
using NetToolBox.Search.Abstractions;
using NetToolBox.Search.AzureCognitiveSearch;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureSearchFactoryServiceCollectionExtensions
    {
        /// <summary>
        /// Add AzureSearch Services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configurationSection">Point to a section of the configuration that can be bound to AzureSearchSettings</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureSearch(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.AddSingleton<AzureSearchFactory>();
            services.AddSingleton<ISearchClient, AzureSearchClient>();
            services.Configure<AzureSearchSettings>(configurationSection);
            services.AddMemoryCache();
            return services;
        }
    }
}