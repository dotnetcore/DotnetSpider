using Microsoft.Extensions.DependencyInjection;
using Quartz.Logging;

namespace DotnetSpider.Portal
{
	public static class QuartzServiceCollectionExtensions
	{
		public static IServiceCollection AddQuartz(this IServiceCollection services)
		{
			services.AddSingleton<QuartzOptions>();
			services.AddSingleton<ILogProvider, QuartzLoggingProvider>();
			return services;
		}
	}
}