using DotnetSpider.AgentRegister.Store;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.AgentRegister
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAgentRegister(this IServiceCollection services)
        {
            services.AddSingleton<IAgentStore, MemoryAgentStore>();
            services.AddHostedService<AgentRegisterService>();
            return services;
        }
    }
}