using DotnetSpider.AgentRegister.Store;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.AgentRegister
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddAgentRegister<T>(this IServiceCollection services)
			where T : class, IAgentStore
		{
			services.AddSingleton<IAgentStore, T>();
			services.AddHostedService<AgentRegisterService>();
			return services;
		}
	}
}
