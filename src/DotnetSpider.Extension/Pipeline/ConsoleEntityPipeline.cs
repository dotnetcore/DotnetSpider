using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DotnetSpider.Extraction.Model;
using DotnetSpider.Common;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// Print datas in console
	/// Usually used in test.
	/// </summary>
	public class ConsoleEntityPipeline : ModelPipeline
	{
		/// <summary>
		/// 打印爬虫实体解析器解析到的实体数据结果到控制台
		/// </summary>
		/// <param name="model">爬虫实体类的名称</param>
		/// <param name="datas">实体类数据</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		protected override int Process(IModel model, IList<dynamic> datas, ILogger logger, dynamic sender = null)
		{
			if (datas == null || datas.Count == 0)
			{
				return 0;
			}

			foreach (var data in datas)
			{
				Console.WriteLine($"Store: {JsonConvert.SerializeObject(data)}");
			}
			return datas.Count;
		}
	}
}
