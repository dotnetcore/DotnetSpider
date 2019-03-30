using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider
{
    public class StatisticsBuilder
    {
        public IServiceCollection Services { get; }
        
        public StatisticsBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}