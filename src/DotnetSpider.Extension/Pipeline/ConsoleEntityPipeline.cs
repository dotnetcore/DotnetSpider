using System;
using System.Collections.Generic;
using DotnetSpider.Core;
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
			if (metadata.Schema == null)
			{
				Spider.Log("Miss pipeline because: Schema is necessary", LogLevel.Warn);
				IsEnabled = false;
			}
		}

		public override void Process(List<JObject> datas)
		{
			foreach (var data in datas)
			{
				Console.WriteLine(data.ToString());
			}
		}

		public override BaseEntityPipeline Clone()
		{
			return new ConsoleEntityPipeline();
		}
	}
}
