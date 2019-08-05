using System;
using DotnetSpider.Common;
using DotnetSpider.DataFlow;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider
{
	public class SpiderProvider
	{
		private readonly IServiceProvider _serviceProvider;

		public SpiderProvider(IServiceProvider serviceProvider)
		{
			Check.NotNull(serviceProvider, nameof(serviceProvider));
			_serviceProvider = serviceProvider;
		}

		public T Create<T>() where T : Spider
		{
			return _serviceProvider.GetRequiredService<T>();
		}

		public Spider Create(Type type)
		{
			var spiderType = typeof(Spider);
			if (!spiderType.IsAssignableFrom(type))
			{
				throw new SpiderException($"{type} is not a spider implement");
			}

			return (Spider) _serviceProvider.GetRequiredService(type);
		}

		public T GetRequiredService<T>()
		{
			return _serviceProvider.GetRequiredService<T>();
		}

		public IServiceProvider CreateScopeServiceProvider()
		{
			return _serviceProvider.CreateScope().ServiceProvider;
		}
	}
}
