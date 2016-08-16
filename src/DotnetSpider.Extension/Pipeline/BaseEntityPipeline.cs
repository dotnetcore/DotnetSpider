using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using Newtonsoft.Json.Linq;
using NLog;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityPipeline : IEntityPipeline
	{
		public ISpider Spider { get; protected set; }
		public bool IsEnabled { get; protected set; } = true;

		protected static ILogger Logger = LogManager.GetCurrentClassLogger();

		public virtual void Dispose()
		{
		}

		public abstract void InitiEntity(EntityMetadata metadata);

		public virtual void InitPipeline(ISpider spider)
		{
			Spider = spider;
		}

		public abstract void Process(List<JObject> datas);

		public abstract BaseEntityPipeline Clone();
	}
}
