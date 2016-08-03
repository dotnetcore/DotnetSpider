using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Java2Dotnet.Spider.Core;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public abstract class EntityBasePipeline : IEntityPipeline
	{
		public ISpider Spider { get; protected set; }

		public virtual void Dispose()
		{
		}

		public virtual void InitPipeline(ISpider spider)
		{
			Spider = spider;
		}

		public abstract void Process(List<JObject> datas);
	}
}
