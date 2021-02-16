using System.IO;
using System.Threading.Tasks;
using DotnetSpider.DataFlow.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace DotnetSpider.DataFlow
{
	/// <summary>
	/// 文件保存解析结果(所有解析结果)
	/// 保存路径: [当前程序运行目录]/files/[任务标识]/[request.hash].data
	/// </summary>
	public class FileStorage : FileStorageBase
	{
		public static IDataFlow CreateFromOptions(IConfiguration configuration)
		{
			return new FileStorage();
		}

		public override async Task HandleAsync(DataFlowContext context)
		{
			if (IsNullOrEmpty(context))
			{
				Logger.LogWarning("数据流上下文不包含解析结果");
				return;
			}

			var file = Path.Combine(GetDataFolder(context.Request.Owner),
				$"{context.Request.Hash}.json");
			using var writer = OpenWrite(file);
			var items = context
				.GetData();
			await writer.WriteLineAsync(System.Text.Json.JsonSerializer.Serialize(new
			{
				uri = context.Request.RequestUri.ToString(), data = items
			}));
		}
	}
}
