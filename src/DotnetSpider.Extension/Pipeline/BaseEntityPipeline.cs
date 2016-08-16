using System;
using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using Newtonsoft.Json.Linq;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityPipeline : IEntityPipeline, ICloneable
	{
		public ISpider Spider { get; protected set; }

		public abstract object Clone();
		//{
			//return MemberwiseClone();
		//}

		public virtual void Dispose()
		{
		}

		public abstract void InitiEntity(EntityMetadata metadata);

		public virtual void InitPipeline(ISpider spider)
		{
			Spider = spider;
		}

		public abstract void Process(List<JObject> datas);
	}
}
