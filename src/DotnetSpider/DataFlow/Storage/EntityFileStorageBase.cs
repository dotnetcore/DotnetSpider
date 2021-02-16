using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 实体解析结果的文件存储器
	/// </summary>
	public abstract class EntityFileStorageBase : EntityStorageBase
	{
		private readonly ConcurrentDictionary<Type, TableMetadata> _tableMetadatas =
			new();

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
		/// <param name="owner">任务标识</param>
		/// <returns></returns>
		protected virtual string GetDataFolder(string owner)
		{
			var path = Path.Combine(Folder, owner);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		protected abstract Task HandleAsync(DataFlowContext context, TableMetadata tableMetadata, IEnumerable entities);

		protected override async Task HandleAsync(DataFlowContext context, IDictionary<Type, ICollection<dynamic>> entities)
		{
			foreach (var kv in entities)
			{
				var list = (IList)kv.Value;
				var tableMetadata = _tableMetadatas.GetOrAdd(kv.Key,
					_ => ((IEntity)list[0]).GetTableMetadata());
				await HandleAsync(context, tableMetadata, list);
			}
		}

		/// <summary>
		/// 获取数据文件路径
		/// </summary>
		/// <param name="context">数据上下文件</param>
		/// <param name="tableMetadata">表元数据</param>
		/// <param name="extension">文件扩展名</param>
		/// <returns></returns>
		protected virtual string GetDataFile(DataFlowContext context, TableMetadata tableMetadata, string extension)
		{
			return Path.Combine(GetDataFolder(context.Request.Owner),
				$"{GenerateFileName(tableMetadata)}.{extension}");
		}

		protected virtual StreamWriter OpenWrite(DataFlowContext context, TableMetadata tableMetadata, string extension)
		{
			var path = GetDataFile(context, tableMetadata, extension);
			var folder = Path.GetDirectoryName(path);
			if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			return new StreamWriter(File.OpenWrite(path), Encoding.UTF8);
		}

		protected virtual string GenerateFileName(TableMetadata tableMetadata)
		{
			return string.IsNullOrWhiteSpace(tableMetadata.Schema.Database)
				? $"{tableMetadata.Schema.Table}"
				: $"{tableMetadata.Schema.Database}.{tableMetadata.Schema.Table}";
		}
	}
}
