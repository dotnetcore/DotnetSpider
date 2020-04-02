using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Agent
{
	public class DownloaderFactory
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ConcurrentDictionary<string, IDownloader> _dict;

		public DownloaderFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_dict = new ConcurrentDictionary<string, IDownloader>();
		}

		public IDownloader Create(string type)
		{
			var downloader = _dict.GetOrAdd(type, t =>
			{
				var downloaderList = _serviceProvider.GetServices(typeof(IDownloader));
				if (downloaderList != null)
				{
					foreach (var x in downloaderList)
					{
						if (x.GetType().Name.StartsWith(type))
						{
							return (IDownloader)x;
						}
					}
				}

				return null;
			});

			return downloader;
		}

		public IEnumerable<string> GetAllDownloaderNames()
		{
			var downloaderList = _serviceProvider.GetServices(typeof(IDownloader));
			if (downloaderList != null)
			{
				return downloaderList.Select(x => x.GetType().Name.Replace("Downloader", ""));
			}

			return Enumerable.Empty<string>();
		}
	}
}
