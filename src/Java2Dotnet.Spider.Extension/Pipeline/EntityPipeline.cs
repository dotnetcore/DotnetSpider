using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Pipeline;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Pipeline
{
	public class EntityPipeline : CachedPipeline
	{
		private readonly IEntityPipeline _pipeline;
		private readonly string _entityName;

		public EntityPipeline(string entityName, IEntityPipeline pipeline)
		{
			_entityName = entityName;
			_pipeline = pipeline;
			pipeline.Initialize();
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
				_pipeline.Process(list, spider);
			}
		}
	}
}
