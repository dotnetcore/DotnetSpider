using System.IO;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.DataFlow.Storage.Model;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// JSON 文件保存解析(实体)结果
	/// 保存路径: [当前程序运行目录]/files/[任务标识]/[request.hash].data
	/// </summary>
	public class JsonEntityFileStorage : EntityFileStorageBase
	{
		/// <summary>
		/// 根据配置返回存储器
		/// </summary>
		/// <param name="options">配置</param>
		/// <returns></returns>
		public static JsonEntityFileStorage CreateFromOptions(SpiderOptions options)
		{
			return new JsonEntityFileStorage();
		}

		protected override async Task<DataFlowResult> Store(DataFlowContext context)
		{
			foreach (var item in context.GetParseItems())
			{
				var tableMetadata = (TableMetadata) context[item.Key];
				StreamWriter writer = CreateOrOpen(context, tableMetadata, "json");
				await writer.WriteLineAsync(JsonConvert.SerializeObject(item.Value));
			}

			return DataFlowResult.Success;
		}
	}
}