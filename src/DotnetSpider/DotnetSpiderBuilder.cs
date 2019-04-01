using DotnetSpider.Data;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider
{
	public class DotnetSpiderBuilder
	{
		public DotnetSpiderBuilder(IServiceCollection services)
		{
			Check.NotNull(services, nameof(services));
			Services = services;
		}

		public IServiceCollection Services { get; }
	}
}