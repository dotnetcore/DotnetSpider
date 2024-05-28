using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetSpider.MessageQueue;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocalMQ(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<IMessageQueue, LocalMQ>();
        return serviceCollection;
    }
}
