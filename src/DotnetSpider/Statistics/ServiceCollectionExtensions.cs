using DotnetSpider.Statistics.Store;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Statistics
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStatistics(this IServiceCollection services)
        {
            services.AddSingleton<IStatisticsStore, MemoryStatisticsStore>();
            services.AddHostedService<StatisticsService>();
            return services;
        }
    }
}