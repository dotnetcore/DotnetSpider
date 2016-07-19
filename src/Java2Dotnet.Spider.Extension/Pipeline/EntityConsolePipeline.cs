using System;
using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Newtonsoft.Json.Linq;
#if NET_CORE
using Java2Dotnet.Spider.Log;
#endif

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	/// <summary>
	/// Print page model in console
	/// Usually used in test.
	/// </summary>
	public class EntityConsolePipeline : IEntityPipeline
	{
		public void Dispose()
		{
		}

		public void Initialize()
		{
		}

		public void Process(List<JObject> datas, ISpider spider)
		{
			foreach (var data in datas)
			{
				Console.WriteLine(data.ToString());
			}
		}
	}
}
