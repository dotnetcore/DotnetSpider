using DotnetSpider.Statistics.Store;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Statistics
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddStatistics<T>(this IServiceCollection services)
			where T : class, IStatisticsStore
		{
			services.AddSingleton<IStatisticsStore, T>();
			services.AddHostedService<StatisticsService>();
			return services;
		}
	}
}
