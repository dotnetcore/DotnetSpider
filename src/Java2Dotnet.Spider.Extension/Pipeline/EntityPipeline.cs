using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Pipeline;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public class EntityPipeline : CachedPipeline
	{
		private readonly List<IEntityPipeline> _pipelines;
		private readonly string _entityName;

		public EntityPipeline(string entityName, List<IEntityPipeline> pipelines)
		{
			_entityName = entityName;
			_pipelines = pipelines;
			foreach (var pipeline in pipelines)
			{
				pipeline.Initialize();
			}
		}

		protected override void Process(List<ResultItems> resultItemsList, ISpider spider)
		{
			if (resultItemsList == null || resultItemsList.Count == 0)
			{
				return;
			}

			List<JObject> list = new List<JObject>();
			foreach (var resultItems in resultItemsList)
			{
				dynamic data = resultItems.GetResultItem(_entityName);

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
			}

			if (list.Count > 0)
			{
				foreach (var pipeline in _pipelines)
				{
					pipeline.Process(list, spider);
				}
			}
		}
	}
}
