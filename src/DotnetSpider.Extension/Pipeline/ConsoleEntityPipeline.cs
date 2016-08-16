using System;
using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// Print page model in console
	/// Usually used in test.
	/// </summary>
	public class ConsoleEntityPipeline : BaseEntityPipeline
	{
		public override void InitiEntity(EntityMetadata metadata)
		{
		}

		public override object Clone()
		{
			return new ConsoleEntityPipeline();
		}


		public override void Process(List<JObject> datas)
		{
			foreach (var data in datas)
			{
				Console.WriteLine(data.ToString());
			}
		}
	}
}
