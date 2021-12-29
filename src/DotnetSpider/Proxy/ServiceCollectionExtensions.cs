using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;

namespace DotnetSpider.Proxy
{
	public static class ServiceCollectionExtensions
	{
		public static Builder UseProxy<TProxySupplier>(this Builder builder, Action<ProxyOptions> configure)
			where TProxySupplier : class, IProxySupplier
		{
			builder.Properties["UserProxy"] = "true";
			builder.ConfigureServices(x =>
			{
				x.AddProxy<TProxySupplier>(configure);
			});
			return builder;
		}

		public static Builder UseProxy<TProxySupplier, TProxyValidator>(this Builder builder,
			Action<ProxyOptions> configure)
			where TProxySupplier : class, IProxySupplier
			where TProxyValidator : class, IProxyValidator
		{
			builder.Properties["UserProxy"] = "true";
			builder.ConfigureServices(x =>
			{
				x.AddProxy<TProxySupplier, TProxyValidator>(configure);
			});
			return builder;
		}

		public static Builder UseProxy<TProxySupplier, TProxyValidator, THttpMessageHandlerBuilder>(
			this Builder builder, Action<ProxyOptions> configure)
			where TProxySupplier : class, IProxySupplier
			where TProxyValidator : class, IProxyValidator
			where THttpMessageHandlerBuilder : HttpMessageHandlerBuilder
		{
			builder.Properties["UserProxy"] = "true";
			builder.ConfigureServices(x =>
			{
				x.AddProxy<TProxySupplier, TProxyValidator, THttpMessageHandlerBuilder>(configure);
			});
			return builder;
		}

		public static IServiceCollection AddProxy<TProxySupplier>(this IServiceCollection serviceCollection,
			Action<ProxyOptions> configure)
			where TProxySupplier : class, IProxySupplier
		{
			serviceCollection.AddProxy<TProxySupplier, DefaultProxyValidator>(configure);
			return serviceCollection;
		}

		public static IServiceCollection AddProxy<TProxySupplier, TProxyValidator>(
			this IServiceCollection serviceCollection, Action<ProxyOptions> configure)
			where TProxySupplier : class, IProxySupplier
			where TProxyValidator : class, IProxyValidator
		{
			serviceCollection.AddProxy<TProxySupplier, TProxyValidator, ProxyHttpMessageHandlerBuilder>(configure);
			return serviceCollection;
		}

		public static IServiceCollection AddProxy<TProxySupplier, TProxyValidator, THttpMessageHandlerBuilder>(
			this IServiceCollection serviceCollection, Action<ProxyOptions> configure)
			where TProxySupplier : class, IProxySupplier
			where TProxyValidator : class, IProxyValidator
			where THttpMessageHandlerBuilder : HttpMessageHandlerBuilder
		{
			serviceCollection.Configure(configure);
			serviceCollection.AddSingleton<IProxySupplier, TProxySupplier>();
			serviceCollection.AddSingleton<IProxyValidator, TProxyValidator>();
			serviceCollection.AddSingleton<IProxyService, ProxyService>();
			serviceCollection.AddHostedService<ProxyBackgroundService>();
			serviceCollection.AddTransient<HttpMessageHandlerBuilder, THttpMessageHandlerBuilder>();
			return serviceCollection;
		}
	}
}
