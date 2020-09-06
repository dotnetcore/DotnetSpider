using DotnetSpider.AgentCenter.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetSpider.AgentCenter
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAgentCenter<T>(this IServiceCollection services)
			where T : class, IAgentStore
		{
			services.TryAddSingleton<IAgentStore, T>();
			services.AddHostedService<AgentCenterService>();
			return services;
		}
	}
}
