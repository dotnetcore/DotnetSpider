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
			builder.Properties["UserProxy"] = "true";
			builder.ConfigureServices(x =>
			{
				x.AddProxy<TProxySupplier>();
			});
			return builder;
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

		public static Builder UseProxy<TProxySupplier, TProxyValidator, THttpMessageHandlerBuilder>(
			this Builder builder)
			where TProxySupplier : class, IProxySupplier
			where TProxyValidator : class, IProxyValidator
			where THttpMessageHandlerBuilder : HttpMessageHandlerBuilder
		{
			builder.Properties["UserProxy"] = "true";
			builder.ConfigureServices(x =>
			{
				x.AddProxy<TProxySupplier, TProxyValidator, THttpMessageHandlerBuilder>();
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
			serviceCollection.AddProxy<TProxySupplier, TProxyValidator, ProxyHttpMessageHandlerBuilder>();
			return serviceCollection;
		}

		public static IServiceCollection AddProxy<TProxySupplier, TProxyValidator, THttpMessageHandlerBuilder>(
			this IServiceCollection serviceCollection)
			where TProxySupplier : class, IProxySupplier
			where TProxyValidator : class, IProxyValidator
			where THttpMessageHandlerBuilder : HttpMessageHandlerBuilder
		{
			serviceCollection.TryAddSingleton<IProxySupplier, TProxySupplier>();
			serviceCollection.TryAddSingleton<IProxyValidator, TProxyValidator>();
			serviceCollection.TryAddSingleton<IProxyService, ProxyService>();
			serviceCollection.AddHostedService<ProxyBackgroundService>();
			serviceCollection.AddTransient<HttpMessageHandlerBuilder, THttpMessageHandlerBuilder>();
			return serviceCollection;
		}
	}
}
