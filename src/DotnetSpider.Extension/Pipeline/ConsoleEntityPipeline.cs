using System;
using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using Newtonsoft.Json.Linq;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// Print page model in console
	/// Usually used in test.
	/// </summary>
	public class ConsoleEntityPipeline : BaseEntityPipeline
	{
		public override void AddEntity(EntityMetadata metadata)
		{

		}

		public override void Process(string entityName, List<JObject> datas)
		{
			foreach (var data in datas)
			{
				Console.WriteLine($"{entityName}: {data.ToString()}");
			}
		}
	}
}
