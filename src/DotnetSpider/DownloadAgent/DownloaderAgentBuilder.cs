using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.DownloadAgent
{
	public class DownloaderAgentBuilder
	{
		public IServiceCollection Services { get; }
		
		public DownloaderAgentBuilder(IServiceCollection services)
		{
			Services = services;
		}
	}
}