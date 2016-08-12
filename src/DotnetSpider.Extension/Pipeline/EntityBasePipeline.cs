using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.ORM;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class EntityBasePipeline : IEntityPipeline
	{
		public ISpider Spider { get; protected set; }

		public virtual void Dispose()
		{
		}

		public abstract void InitiEntity(Schema schema, EntityMetadata metadata);

		public virtual void InitPipeline(ISpider spider)
		{
			Spider = spider;
		}

		public abstract void Process(List<JObject> datas);
	}
}
