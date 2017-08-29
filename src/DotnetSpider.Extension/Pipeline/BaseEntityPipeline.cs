using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Pipeline;
using System.Collections.Concurrent;

namespace DotnetSpider.Extension.Pipeline
{
	public abstract class BaseEntityPipeline : BasePipeline, IEntityPipeline
	{
		private readonly ConcurrentDictionary<string, EntityDefine> _entities = new ConcurrentDictionary<string, EntityDefine>();

		protected ConcurrentDictionary<string, EntityDefine> Entities => _entities;

		public abstract void Process(string entityName, List<DataObject> items);

		public virtual void AddEntity(EntityDefine entityDefine)
		{
			if (entityDefine == null)
			{
				return;
			}
			_entities.TryAdd(entityDefine.Name, entityDefine);
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
					List<DataObject> list = new List<DataObject>();
					dynamic data = resultItem.GetResultItem(result.Key);

					if (data != null)
					{
						if (data is DataObject)
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
