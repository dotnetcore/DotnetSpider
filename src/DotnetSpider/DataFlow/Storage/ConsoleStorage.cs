using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Storage
{
	/// <summary>
	/// 控制台打印解析结果(所有解析结果)
	/// </summary>
	public class ConsoleStorage : StorageBase
	{
		public static IDataFlow CreateFromOptions(IConfiguration configuration)
		{
			return new ConsoleStorage();
		}

		protected override Task StoreAsync(DataFlowContext context)
		{
			var items = context.GetData();

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"{Environment.NewLine}DATA: {JsonConvert.SerializeObject(items)}{Environment.NewLine}");

			return Task.CompletedTask;
		}
	}
}
