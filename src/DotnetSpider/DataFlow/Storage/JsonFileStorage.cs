using System.IO;
using System.Threading.Tasks;
using DotnetSpider.Common;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// JSON 文件保存解析结果(所有解析结果)
	/// 保存路径: [当前程序运行目录]/files/[任务标识]/[request.hash].json
	/// </summary>
	public class JsonFileStorage : FileStorageBase
	{
		/// <summary>
		/// 根据配置返回存储器
		/// </summary>
		/// <param name="options">配置</param>
		/// <returns></returns>
		public static JsonFileStorage CreateFromOptions(SpiderOptions options)
		{
			return new JsonFileStorage();
		}

		protected override async Task<DataFlowResult> Store(DataFlowContext context)
		{
			var items = context.GetData();
			var file = Path.Combine(GetDataFolder(context.Response.Request.OwnerId),
				$"{context.Response.Request.Hash}.json");
			CreateFile(file);
			await Writer.WriteLineAsync(JsonConvert.SerializeObject(items));

			return DataFlowResult.Success;
		}
	}
}