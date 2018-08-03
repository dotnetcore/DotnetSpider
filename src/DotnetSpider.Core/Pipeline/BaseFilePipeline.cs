using System.Collections.Concurrent;
using System.IO;
using System;
using DotnetSpider.Common;

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
			if (string.IsNullOrWhiteSpace(interval))
			{
				RootDataFolder = AppDomain.CurrentDomain.BaseDirectory;
			}
			else
			{
				if (!interval.EndsWith(Env.PathSeperator))
				{
					interval += Env.PathSeperator;
				}

				RootDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, interval);
			}
		}

		public string GetDataFolder(IIdentity sender)
		{
			if (sender == null)
			{
				throw new ArgumentNullException(nameof(sender));
			}
			if (_dataFolderCache.ContainsKey(sender.Identity))
			{
				return _dataFolderCache[sender.Identity];
			}
			else
			{
				var dataFolder = Path.Combine(RootDataFolder, sender.Identity);
				if (!Directory.Exists(dataFolder))
				{
					Directory.CreateDirectory(dataFolder);
				}
				_dataFolderCache.TryAdd(sender.Identity, dataFolder);
				return dataFolder;
			}
		}
	}
}
