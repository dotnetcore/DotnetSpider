using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using DotnetSpider.DataFlow.Storage;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace DotnetSpider.DataFlow
{
	/// <summary>
	/// JSON 文件保存解析(实体)结果
	/// 保存路径: [当前程序运行目录]/files/[任务标识]/[request.hash].data
	/// </summary>
	public class JsonEntityFileStorage : EntityFileStorageBase
	{
		private readonly ConcurrentDictionary<string, StreamWriter> _streamWriters =
			new();

		/// <summary>
		/// 根据配置返回存储器
		/// </summary>
		/// <param name="configuration">配置</param>
		/// <returns></returns>
		public static JsonEntityFileStorage CreateFromOptions(IConfiguration configuration)
		{
			return new();
		}

		public override Task InitializeAsync()
		{
			return Task.CompletedTask;
		}

		protected override async Task HandleAsync(DataFlowContext context, TableMetadata tableMetadata,
			IEnumerable entities)
		{
			var streamWriter = _streamWriters.GetOrAdd(tableMetadata.TypeName,
				_ => OpenWrite(context, tableMetadata, "json"));
			await streamWriter.WriteLineAsync(System.Text.Json.JsonSerializer.Serialize(entities));
		}

		public override void Dispose()
		{
			foreach (var streamWriter in _streamWriters)
			{
				streamWriter.Value.Dispose();
			}

			base.Dispose();
		}
	}
}
