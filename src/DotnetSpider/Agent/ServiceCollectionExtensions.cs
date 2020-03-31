using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Agent
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAgent(this IServiceCollection services)
        {
            services.AddSingleton<HttpClientDownloader>();
            services.AddSingleton<PuppeteerDownloader>();
            services.AddSingleton<PPPoEService>();
            services.AddHostedService<AgentService>();
            return services;
        }
    }
}