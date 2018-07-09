using System.Collections.Concurrent;
using System.IO;

namespace DotnetSpider.Core.Pipeline
{
	/// <summary>
	/// 文件类型数据管理的抽象
	/// </summary>
	public abstract class BaseFilePipeline : BasePipeline
	{
		private readonly ConcurrentDictionary<string, string> _dataFolderCache = new ConcurrentDictionary<string, string>();

		/// <summary>
		/// 数据根目录
		/// </summary>
		protected string RootDataFolder { get; set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="interval">数据根目录与程序运行目录路径的相对值</param>
		protected BaseFilePipeline(string interval)
		{
			InitFolder(interval);
		}

		/// <summary>
		/// 初始化需要使用的文件夹
		/// </summary>
		/// <param name="interval">数据根目录与程序运行目录路径的相对值</param>
		protected void InitFolder(string interval)
		{
			if (string.IsNullOrWhiteSpace(interval))
			{
				throw new SpiderException("Interval path should not be null");
			}
			if (!interval.EndsWith(Env.PathSeperator))
			{
				interval += Env.PathSeperator;
			}

			RootDataFolder = Path.Combine(Env.BaseDirectory, interval);
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
