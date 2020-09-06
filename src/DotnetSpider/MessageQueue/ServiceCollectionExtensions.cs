using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.MessageQueue
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddInProcessMessageQueue(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IMessageQueue, MessageQueue>();
			return serviceCollection;
		}
	}
}
