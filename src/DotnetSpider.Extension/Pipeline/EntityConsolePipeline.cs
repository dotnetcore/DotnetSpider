using System;
using System.Collections.Generic;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	/// <summary>
	/// Print page model in console
	/// Usually used in test.
	/// </summary>
	public class EntityConsolePipeline : EntityBasePipeline
	{
		public override void InitiEntity(Schema schema, EntityMetadata metadata)
		{
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
