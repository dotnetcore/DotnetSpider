using DotnetSpider.Broker.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Broker
{
	public static class ServiceCollectionExtensions
	{
		public static IMvcBuilder AddBrokerService(this IMvcBuilder builder)
		{
			builder.Services.AddTransient<IWorkerService, WorkerService>();
			builder.Services.AddTransient<INodeService, NodeService>();
			builder.Services.AddTransient<INodeStatusService, NodeStatusService>();

			return builder;
		}
	}
}
