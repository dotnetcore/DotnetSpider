using System;
using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using System.Linq;
using DotnetSpider.Core;
using Newtonsoft.Json;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// Print page model in console
	/// Usually used in test.
	/// </summary>
	public class ConsoleEntityPipeline : BaseEntityPipeline
	{
		public override void AddEntity(IEntityDefine metadata)
		{
		}

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
