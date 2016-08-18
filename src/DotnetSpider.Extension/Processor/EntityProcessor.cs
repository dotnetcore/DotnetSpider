using System.Collections.Generic;
using DotnetSpider.Core;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using Site = DotnetSpider.Core.Site;
using Newtonsoft.Json.Linq;


namespace DotnetSpider.Extension.Processor
{
	public class EntityProcessor : IPageProcessor
	{
		protected readonly IList<IEntityExtractor> EntityExtractorList = new List<IEntityExtractor>();
		private readonly EntitySpider _spiderContext;

		public EntityProcessor(EntitySpider spiderContext)
		{
			Site = spiderContext.Site;
			_spiderContext = spiderContext;
		}


		public void AddEntity(EntityMetadata entityDefine)
		{
			EntityExtractorList.Add(GenerateExtractor(entityDefine));
		}

		private IEntityExtractor GenerateExtractor(EntityMetadata entityDefine)
		{
			return new EntityExtractor(entityDefine.Entity.Name, _spiderContext.GlobalValues, entityDefine);
		}

		public void Process(Page page)
		{
			foreach (IEntityExtractor pageModelExtractor in EntityExtractorList)
			{
				List<JObject> list = pageModelExtractor.Process(page);

				if (list == null || list.Count == 0)
				{
					continue;
				}

				if (pageModelExtractor.DataHandler != null)
				{
					foreach (var data in list)
					{
						pageModelExtractor.DataHandler.Handle(data, page);
					}
				}

				page.AddResultItem(pageModelExtractor.EntityMetadata.Entity.Name, list);
			}

			if (page.ResultItems.Results.Count == 0)
			{
				page.ResultItems.IsSkip = true;
			}
		}

		public Site Site { get; set; }
	}
}
