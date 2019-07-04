using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider
{
	public class DownloadAgentBuilder
	{
		public IServiceCollection Services { get; }
		
		public DownloadAgentBuilder(IServiceCollection services)
		{
			Services = services;
		}
	}
}