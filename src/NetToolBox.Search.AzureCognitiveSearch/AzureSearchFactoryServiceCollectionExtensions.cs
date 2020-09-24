using Microsoft.Extensions.Configuration;
using NetToolBox.Search.AzureCognitiveSearch;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureSearchFactoryServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureSearch(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<AzureSearchSettings>(configurationSection);
            return services;
        }
    }
}