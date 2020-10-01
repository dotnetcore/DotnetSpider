using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;

namespace DotnetSpider.Proxy
{
	public static class ServiceCollectionExtensions
	{
		public static Builder UseProxy<TProxySupplier>(this Builder builder)
			where TProxySupplier : class, IProxySupplier
		{
			return builder.UseProxy<TProxySupplier, DefaultProxyValidator>();
		}

		public static Builder UseProxy<TProxySupplier, TProxyValidator>(this Builder builder)
			where TProxySupplier : class, IProxySupplier
			where TProxyValidator : class, IProxyValidator
		{
			builder.Properties["UserProxy"] = "true";
			builder.ConfigureServices(x =>
			{
				x.AddProxy<TProxySupplier, TProxyValidator>();
			});
			return builder;
		}

		public static IServiceCollection AddProxy<TProxySupplier>(this IServiceCollection serviceCollection)
			where TProxySupplier : class, IProxySupplier
		{
			serviceCollection.AddProxy<TProxySupplier, DefaultProxyValidator>();
			return serviceCollection;
		}

		public static IServiceCollection AddProxy<TProxySupplier, TProxyValidator>(
			this IServiceCollection serviceCollection)
			where TProxySupplier : class, IProxySupplier
			where TProxyValidator : class, IProxyValidator
		{
			serviceCollection.TryAddSingleton<IProxySupplier, TProxySupplier>();
			serviceCollection.TryAddSingleton<IProxyValidator, TProxyValidator>();
			serviceCollection.TryAddSingleton<IProxyService, ProxyService>();
			serviceCollection.AddHostedService<ProxyBackgroundService>();
			serviceCollection.AddTransient<HttpMessageHandlerBuilder, ProxyHttpMessageHandlerBuilder>();

			return serviceCollection;
		}
	}
}
