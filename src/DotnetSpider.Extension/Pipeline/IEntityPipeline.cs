using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public interface IEntityPipeline : IDisposable
	{
		ISpider Spider { get; }
		void InitPipeline(ISpider spider);
		void Process(List<JObject> datas);
	}
}
