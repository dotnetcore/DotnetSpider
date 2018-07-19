//using DotnetSpider.Core;
//using DotnetSpider.Core.Downloader;
//using DotnetSpider.Extension.Model;
//using DotnetSpider.Extension.Pipeline;

//namespace DotnetSpider.Extension.Downloader
//{
//	/// <summary>
//	/// Save download content to database.
//	/// </summary>
//	public class DownloadCacheHandler : AfterDownloadCompleteHandler
//	{
//		private readonly BaseEntityPipeline _pipeline;

//		/// <summary>
//		/// 构造方法
//		/// </summary>
//		/// <param name="pipeline">数据管道</param>
//		public DownloadCache(BaseEntityPipeline pipeline)
//		{
//			if (pipeline == null)
//			{
//				_pipeline = DbModelPipeline.GetPipelineFromAppConfig() as DbModelPipeline;
//			}
//			else
//			{
//				_pipeline = pipeline;
//			}

//			if (_pipeline == null)
//			{
//				throw new SpiderException("StorageCache's pipeline unfound");
//			}
//			_pipeline.AddEntity(new EntityDefine<DownloadCacheData>());
//			_pipeline.Init();
//		}

//		/// <summary>
//		/// 把页面数据存到数据管道中
//		/// </summary>
//		/// <param name="page">页面数据</param>
//		/// <param name="downloader">下载器</param>
//		/// <param name="spider">爬虫</param>
//		public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
//		{
//			var cache = new DownloadCacheData();
//			cache.Identity = spider.Identity;
//			cache.TaskId = spider.TaskId;
//			cache.Name = spider.Name;
//			cache.Url = page.Url;
//			cache.Content = page.Content;

//			_pipeline.Process(typeof(DownloadCacheData).FullName, new DownloadCacheData[] { cache }, spider);
//		}
//	}
//}
