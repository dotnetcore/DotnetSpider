using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider
{
    public class DownloadCenterBuilder
    {
        public IServiceCollection Services { get; }
        
        public DownloadCenterBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}