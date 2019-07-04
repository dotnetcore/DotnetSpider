using System;
using DotnetSpider.Core;
using DotnetSpider.Data;
using DotnetSpider.Downloader;
using DotnetSpider.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider
{
	public class SpiderProvider
	{
		private readonly IServiceProvider _serviceProvider;
		private bool _isRunning;

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
				throw new SpiderException($"类型 {type} 不是爬虫类型");
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

		public void Start()
		{
			if (!_isRunning)
			{
				_serviceProvider.GetService<IDownloadAgentRegisterCenter>()?.StartAsync(default).ConfigureAwait(false).GetAwaiter();
				_serviceProvider.GetService<IDownloaderAgent>()?.StartAsync(default).ConfigureAwait(false).GetAwaiter();
				_serviceProvider.GetService<IStatisticsCenter>()?.StartAsync(default).ConfigureAwait(false).GetAwaiter();
				_isRunning = true;
			}
		}
	}
}