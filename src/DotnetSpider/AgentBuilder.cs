using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider
{
	public class AgentBuilder
	{
		public IServiceCollection Services { get; }
		
		public AgentBuilder(IServiceCollection services)
		{
			Services = services;
		}
	}
}