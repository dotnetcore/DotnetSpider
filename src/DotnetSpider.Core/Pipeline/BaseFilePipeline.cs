using System.Collections.Concurrent;
using System.IO;

namespace DotnetSpider.Core.Pipeline
{
	public abstract class BaseFilePipeline : BasePipeline
	{
		private readonly ConcurrentDictionary<string, string> _dataFolderCache = new ConcurrentDictionary<string, string>();
		public string RootDataFolder { get; protected set; }
		public string Interval { get; protected set; }

		protected BaseFilePipeline() { }

		protected BaseFilePipeline(string interval)
		{
			InitFolder(interval);
		}

		protected void InitFolder(string interval)
		{
			if (string.IsNullOrEmpty(interval) || string.IsNullOrWhiteSpace(interval))
			{
				throw new SpiderException("Interval path should not be null.");
			}
			if (!interval.EndsWith(Env.PathSeperator))
			{
				interval += Env.PathSeperator;
			}

			RootDataFolder = Path.Combine(Env.BaseDirectory, interval);
			Interval = interval;
		}

		internal string GetDataFolder(ISpider spider)
		{
			if (_dataFolderCache.ContainsKey(spider.Identity))
			{
				return _dataFolderCache[spider.Identity];
			}
			else
			{
				var dataFolder = Path.Combine(RootDataFolder, spider.Identity);
				if (!Directory.Exists(dataFolder))
				{
					Directory.CreateDirectory(dataFolder);
				}
				_dataFolderCache.TryAdd(spider.Identity, dataFolder);
				return dataFolder;
			}
		}
	}
}
