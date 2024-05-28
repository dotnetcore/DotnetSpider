using DotnetSpider.Statistic.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetSpider.Statistic;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStatisticHostService<T>(this IServiceCollection services)
        where T : class, IStatisticStore
    {
            services.TryAddSingleton<IStatisticStore, T>();
            services.AddHostedService<StatisticHostService>();
            return services;
        }
}
