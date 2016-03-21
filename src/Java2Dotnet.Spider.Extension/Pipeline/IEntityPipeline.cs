using System;
using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public interface IEntityPipeline : IDisposable
	{
		void Initialize();
		void Process(List<JObject> datas, ISpider spider);
	}
}
