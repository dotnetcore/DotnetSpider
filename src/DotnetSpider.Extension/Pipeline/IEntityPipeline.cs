using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;
using DotnetSpider.Core.Pipeline;

namespace DotnetSpider.Extension.Pipeline
{
	public interface IEntityPipeline
	{
		void Process(string entityName, List<JObject> datas);
	}
}
