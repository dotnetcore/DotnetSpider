using Microsoft.Extensions.DependencyInjection;
using System;

namespace DotnetSpider.Core
{
	public class IocExtension
	{
		private static IServiceProvider _serviceProvider;

		public static IServiceCollection ServiceCollection { get; set; } = new ServiceCollection();

		public static IServiceProvider ServiceProvider => _serviceProvider ?? (_serviceProvider = ServiceCollection.BuildServiceProvider());
	}
}
