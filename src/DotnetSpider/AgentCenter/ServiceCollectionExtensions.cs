using DotnetSpider.AgentCenter.Store;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.AgentCenter
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAgentCenter<TAgentStore>(this IServiceCollection services)
			where TAgentStore : class, IAgentStore
		{
			services.AddSingleton<IAgentStore, TAgentStore>();
			services.AddHostedService<AgentCenterService>();
			return services;
		}
	}
}
