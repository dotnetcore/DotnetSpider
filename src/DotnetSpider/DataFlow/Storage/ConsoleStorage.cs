using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
			var items = context.GetData().Where(x => !ReferenceEquals(x.Key, Consts.ResponseBytes));

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"{Environment.NewLine}DATA: {JsonConvert.SerializeObject(items)}{Environment.NewLine}");

			return Task.CompletedTask;
		}
	}
}
