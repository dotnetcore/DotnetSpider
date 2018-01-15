using System;
using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using System.Linq;
using DotnetSpider.Core;
using Newtonsoft.Json;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// Print datas in console
	/// Usually used in test.
	/// </summary>
	public class ConsoleEntityPipeline : BaseEntityPipeline
	{
		/// <summary>
		/// 添加爬虫实体类的定义, Console pipeline不需要
		/// </summary>
		/// <param name="entityDefine">爬虫实体类的定义</param>
		public override void AddEntity(IEntityDefine entityDefine)
		{
		}

		/// <summary>
		/// 打印爬虫实体解析器解析到的实体数据结果到控制台
		/// </summary>
		/// <param name="entityName">爬虫实体类的名称</param>
		/// <param name="datas">实体类数据</param>
		/// <param name="spider">爬虫</param>
		/// <returns>最终影响结果数量(如数据库影响行数)</returns>
		public override int Process(string entityName, IEnumerable<dynamic> datas, ISpider spider)
		{
			foreach (var data in datas)
			{
				Console.WriteLine($"{entityName}: {JsonConvert.SerializeObject(data)}");
			}
			return datas.Count();
		}
	}
}
