using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 解析结果的文件存储器
	/// </summary>
	public abstract class FileStorageBase : DataFlowBase
	{
		private readonly object _locker = new();

		/// <summary>
		/// 存储的根文件夹
		/// </summary>
		protected string Folder { get; private set; }

		public override Task InitializeAsync()
		{
			Folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files");
			if (!Directory.Exists(Folder))
			{
				Directory.CreateDirectory(Folder);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// 获取存储文件夹
		/// </summary>
		/// <param name="owner">任务标识</param>
		/// <returns></returns>
		protected string GetDataFolder(string owner)
		{
			var path = Path.Combine(Folder, owner);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		/// <summary>
		/// 创建文件写入器
		/// </summary>
		/// <param name="file"></param>
		protected StreamWriter OpenWrite(string file)
		{
			lock (_locker)
			{
				return new StreamWriter(File.OpenWrite(file), Encoding.UTF8);
			}
		}
	}
}
