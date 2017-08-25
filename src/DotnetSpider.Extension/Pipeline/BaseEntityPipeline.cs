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
		private readonly ConcurrentDictionary<string, Entity> _entities = new ConcurrentDictionary<string, Entity>();

		public abstract void Process(string entityName, List<JObject> items);

		protected ConcurrentDictionary<string, Entity> Entities => _entities;

		public virtual void AddEntity(Entity entity)
		{
			if (entity.Table == null)
			{
				return;
			}
			_entities.TryAdd(entity.Name, entity);
		}

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
