using System;
using System.Threading.Tasks;
using DotnetSpider.Core;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 控制台打印解析结果(所有解析结果)
	/// </summary>
	public class ConsoleStorage : StorageBase
	{
		/// <summary>
		/// 根据配置返回存储器
		/// </summary>
		/// <param name="options">配置</param>
		/// <returns></returns>
		public static ConsoleStorage CreateFromOptions(SpiderOptions options)
		{
			return new ConsoleStorage();
		}

		protected override Task<DataFlowResult> Store(DataFlowContext context)
		{
			var items = context.GetItems();
			Console.WriteLine(JsonConvert.SerializeObject(items));
			return Task.FromResult(DataFlowResult.Success);
		}
	}
}