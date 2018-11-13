using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using DotnetSpider.Extraction.Model;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// Print datas in console
	/// Usually used in test.
	/// </summary>
	public class ConsoleEntityPipeline : EntityPipeline
	{
		/// <summary>
		/// 打印爬虫实体解析器解析到的实体数据结果到控制台
		/// </summary>
		/// <param name="items">实体类数据</param>
		/// <param name="sender">调用方</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		protected override int Process(List<IBaseEntity> items, dynamic sender = null)
		{
			if (items == null)
			{
				return 0;
			}

			foreach (var data in items)
			{
				Console.WriteLine($"Store: {JsonConvert.SerializeObject(data)}");
			}
			return items.Count;
		}
	}
}
