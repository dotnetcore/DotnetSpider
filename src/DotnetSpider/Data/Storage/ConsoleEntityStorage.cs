using System;
using System.Threading.Tasks;
using DotnetSpider.Core;
using Newtonsoft.Json;

namespace DotnetSpider.Data.Storage
{
	/// <summary>
	/// 控制台打印(实体)解析结果
	/// </summary>
	public class ConsoleEntityStorage : StorageBase
	{
		/// <summary>
		/// 根据配置返回存储器
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public static ConsoleEntityStorage CreateFromOptions(ISpiderOptions options)
		{
			return new ConsoleEntityStorage();
		}

		protected override Task<DataFlowResult> Store(DataFlowContext context)
		{
			var items = context.GetItems();
			foreach (var item in items)
			{
				foreach (var data in item.Value)
				{
					Console.WriteLine(JsonConvert.SerializeObject(data));
				}
			}

			return Task.FromResult(DataFlowResult.Success);
		}
	}
}