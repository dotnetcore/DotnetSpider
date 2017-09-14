using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Downloader
{
	/// <summary>
	/// Save download content to database.
	/// </summary>
	public class StorageCache : AfterDownloadCompleteHandler
	{
		private readonly BaseEntityDbPipeline _pipeline;
		private readonly ISpider _spider;

		public StorageCache(ISpider spider)
		{
			_pipeline = BaseEntityDbPipeline.GetPipelineFromAppConfig() as BaseEntityDbPipeline;

			if (_pipeline == null)
			{
				throw new SpiderException("Can not get StorageCache's pipeline.");
			}
			_pipeline.AddEntity(EntityDefine.Parse<CrawlCache>());
			_pipeline.InitPipeline(spider);

			_spider = spider;
		}

		public override void Handle(ref Page page, ISpider spider)
		{
			var cache = new DataObject();
			cache.Add("Identity", _spider.Identity);
			cache.Add("Name", _spider.Name);
			cache.Add("TaskId", _spider.TaskId);
			cache.Add("Url", page.Url);
			cache.Add("Content", page.Content);

			_pipeline.Process(typeof(CrawlCache).FullName, new List<DataObject>
			{
				cache
			});
		}
	}
}
