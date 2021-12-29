using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetSpider.MessageQueue
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddMessageQueue(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddSingleton<IMessageQueue, MessageQueue>();
			return serviceCollection;
		}
	}
}
