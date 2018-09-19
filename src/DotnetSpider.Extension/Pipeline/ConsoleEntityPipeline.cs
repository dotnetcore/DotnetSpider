using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using DotnetSpider.Extraction.Model;
using System.Linq;

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
		/// <param name="model">爬虫实体类的名称</param>
		/// <param name="datas">实体类数据</param>
		/// <param name="logger">日志接口</param>
		/// <param name="sender">调用方</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		protected override int Process(IEnumerable<IBaseEntity> datas, dynamic sender = null)
		{
			if (datas == null)
			{
				return 0;
			}

			foreach (var data in datas)
			{
				Console.WriteLine($"Store: {JsonConvert.SerializeObject(data)}");
			}
			return datas.Count();
		}
	}
}
