using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Impl;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Logging;

namespace DotnetSpider.Portal
{
	public static class QuartzServiceCollectionExtensions
	{
		public static IServiceCollection AddQuartz(this IServiceCollection services)
		{
			services.AddSingleton<QuartzOptions>();
			services.AddSingleton<ILogProvider, QuartzLoggingProvider>();
			services.AddSingleton<IDbProvider, MySqlDbProvider>();
			services.AddSingleton(provider =>
			{
				var options = provider.GetRequiredService<QuartzOptions>();
				var logProvider = provider.GetRequiredService<ILogProvider>();
				QuartzLoggingProvider.CurrentLogProvider = logProvider;
				LogProvider.SetCurrentLogProvider(logProvider);
				options.Properties.Add("quartz.jobStore.useProperties", "true");
				var properties = new NameValueCollection();
				foreach (var property in options.Properties)
				{
					properties.Add(property.Key, property.Value);
				}

				return new StdSchedulerFactory(properties).GetScheduler().GetAwaiter().GetResult();
			});
			return services;
		}
	}
}
