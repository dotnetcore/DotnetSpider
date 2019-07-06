using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.DownloadAgentRegisterCenter
{
	public class DownloadAgentRegisterCenterBuilder
	{
		public IServiceCollection Services { get; }

		public DownloadAgentRegisterCenterBuilder(IServiceCollection services)
		{
			Services = services;
		}
	}
}