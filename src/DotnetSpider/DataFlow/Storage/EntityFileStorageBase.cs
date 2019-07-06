using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using DotnetSpider.DataFlow.Storage.Model;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 实体解析结果的文件存储器
	/// </summary>
	public abstract class EntityFileStorageBase : EntityStorageBase
	{
		protected readonly ConcurrentDictionary<string, StreamWriter> Writers =
			new ConcurrentDictionary<string, StreamWriter>();

		/// <summary>
		/// 存储的根文件夹
		/// </summary>
		protected string Folder { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		protected EntityFileStorageBase()
		{
			Folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files");
			if (!Directory.Exists(Folder))
			{
				Directory.CreateDirectory(Folder);
			}
		}

		/// <summary>
		/// 获取存储文件夹
		/// </summary>
		/// <param name="ownerId">任务标识</param>
		/// <returns></returns>
		protected virtual string GetDataFolder(string ownerId)
		{
			var path = Path.Combine(Folder, ownerId);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		/// <summary>
		/// 获取数据文件路径
		/// </summary>
		/// <param name="dfc">数据上下文件</param>
		/// <param name="tableMetadata">表元数据</param>
		/// <param name="extension">文件扩展名</param>
		/// <returns></returns>
		protected virtual string GetDataFile(DataFlowContext dfc, TableMetadata tableMetadata, string extension)
		{
			return Path.Combine(GetDataFolder(dfc.Response.Request.OwnerId),
				$"{GenerateFileName(tableMetadata)}.{extension}");
		}

		protected virtual StreamWriter CreateOrOpen(DataFlowContext dfc, TableMetadata tableMetadata, string extension)
		{
			var path = GetDataFile(dfc, tableMetadata, extension);
			return Writers.GetOrAdd(path, x =>
			{
				var folder = Path.GetDirectoryName(x);
				if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}

				return new StreamWriter(File.OpenWrite(x), Encoding.UTF8);
			});
		}

		protected virtual string GenerateFileName(TableMetadata tableMetadata)
		{
			return string.IsNullOrWhiteSpace(tableMetadata.Schema.Database)
				? $"{tableMetadata.Schema.Table}"
				: $"{tableMetadata.Schema.Database}.{tableMetadata.Schema.Table}";
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach (var writer in Writers)
			{
				try
				{
					writer.Value.Dispose();
				}
				catch (Exception e)
				{
					Logger?.LogError($"释放文件 {writer.Key} 失败: {e}");
				}
			}
		}
	}
}