using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using Newtonsoft.Json.Linq;
using DotnetSpider.Core.Pipeline;
using System.Collections.Concurrent;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityPipeline : BasePipeline, IEntityPipeline
	{
		protected ConcurrentDictionary<string, Entity> EntityMetadatas = new ConcurrentDictionary<string, Entity>();

		public virtual void AddEntity(Entity metadata)
		{
			if (metadata.Table == null)
			{
				//Spider.Log($"Schema is necessary, Pass {GetType().Name} for {metadata.Entity.Name}.", LogLevel.Warn);
				return;
			}
			EntityMetadatas.TryAdd(metadata.Name, metadata);
		}

		public abstract void Process(string entityName, List<JObject> datas);

		public override void Process(params ResultItems[] resultItems)
		{
			if (resultItems == null || resultItems.Length == 0)
			{
				return;
			}

			foreach (var resultItem in resultItems)
			{
				foreach (var result in resultItem.Results)
				{
					List<JObject> list = new List<JObject>();
					dynamic data = resultItem.GetResultItem(result.Key);

					if (data != null)
					{
						if (data is JObject)
						{
							list.Add(data);
						}
						else
						{
							list.AddRange(data);
						}
					}
					if (list.Count > 0)
					{
						Process(result.Key, list);
					}
				}
			}
		}
	}
}
