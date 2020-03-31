using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SwiftMQ;

namespace DotnetSpider.RabbitMQ
{
	public static class ServiceCollectionExtensions
	{
		public static Builder UseRabbitMQ(this Builder builder, Action<SpiderOptions> configureDelegate = null)
		{
			builder.ConfigureServices(services =>
			{
				var fields = typeof(HostBuilder).GetField("_appConfiguration",
					BindingFlags.NonPublic | BindingFlags.Instance);
				if (fields != null)
				{
					var configuration = (IConfiguration)fields.GetValue(builder);
					services.Configure<RabbitMQOptions>(configuration);
				}

				if (configureDelegate != null)
				{
					services.Configure(configureDelegate);
				}

				services.AddSingleton<IMessageQueue, RabbitMQMessageQueue>();
			});
			return builder;
		}
	}
}
