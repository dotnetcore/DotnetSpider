using System.Collections.Generic;
using DotnetSpider.Core;
using Newtonsoft.Json.Linq;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Scheduler;
using DotnetSpider.Extension.Scheduler;
using DotnetSpider.Extension.Model;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extension.Infrastructure;
using System;

#if !NET_CORE
using DotnetSpider.Extension.Downloader.WebDriver;
using DotnetSpider.Core.Infrastructure;
#endif

namespace DotnetSpider.Extension.Configuration
{
	public class JsonSpiderContext : EntitySpiderBuilder
	{
		public JsonSpiderContext() : base("NULL", Infrastructure.Batch.Now)
		{
		}

		public string LogAndStatusConnectString { get; set; }
		public string RedisConnectString { get; set; }
		public Site Site { get; set; }
		public int CachedSize { get; set; } = 1;
		public JObject Scheduler { get; set; }
		public JObject Downloader { get; set; }
		public List<JObject> Pipelines { get; set; }
		public JObject CookieInjector { get; set; }
		public int EmptySleepTime { get; set; } = 15000;
		public int ThreadNum { get; set; } = 1;
		public int Deep { get; set; } = int.MaxValue;
		public bool SpawnUrl { get; set; } = true;
		public bool SkipWhenResultIsEmpty { get; set; } = false;
		public bool RetryWhenResultIsEmpty { get; set; } = false;

		public string Batch
		{
			get
			{
				return _batch;
			}
			set
			{
				_batch = value;
			}
		}

		public List<Entity> Entities { get; set; }

		private IScheduler GetScheduler(JObject jobject)
		{
			if (jobject == null)
			{
				return new QueueDuplicateRemovedScheduler();
			}

			var schedulerType = jobject.SelectToken("$.Type")?.ToString();

			switch (schedulerType)
			{
				case "Memeroy":
					{
						return new QueueDuplicateRemovedScheduler();
					}
				case "Redis":
					{
						return jobject.ToObject<RedisScheduler>();
					}
			}

			throw new SpiderException("Can't convert Scheduler: " + jobject);
		}

		private List<IPipeline> GetPipepines(List<JObject> pipelines)
		{
			if (pipelines == null || pipelines.Count == 0)
			{
				throw new SpiderException("Missing Pipeline.");
			}

			List<IPipeline> results = new List<IPipeline>();
			foreach (var pipeline in pipelines)
			{
				IPipeline tmp = null;
				var pipelineType = pipeline.SelectToken("$.Type")?.ToString();

				if (pipelineType == null)
				{
					throw new SpiderException("Missing PrepareStartUrls type: " + pipeline);
				}

				switch (pipelineType)
				{
					case "MongoDb":
						{
							tmp = new MongoDbEntityPipeline(pipeline.SelectToken("$.ConnectString").ToString());
							break;
						}
					case "SqlServer":
						{
							var checkIfSaveBeforeUpdate = pipeline.SelectToken("$.checkIfSaveBeforeUpdate");
							tmp = new SqlServerEntityPipeline(pipeline.SelectToken("$.ConnectString").ToString(), checkIfSaveBeforeUpdate?.ToObject<bool>() ?? false);
							break;
						}
					case "MySql":
						{
							var checkIfSaveBeforeUpdate = pipeline.SelectToken("$.checkIfSaveBeforeUpdate");
							tmp = new MySqlEntityPipeline(pipeline.SelectToken("$.ConnectString").ToString(), checkIfSaveBeforeUpdate?.ToObject<bool>() ?? false);
							break;
						}
					case "MySqlFile":
						{
							tmp = new MySqlFileEntityPipeline
							{
								DataFolder = pipeline.SelectToken("$.DataFolder").ToString()
							};
							break;
						}
				}

				if (tmp == null)
				{
					throw new SpiderException("UNSPORT PIPELINE: " + pipeline);
				}
				else
				{
					results.Add(tmp);
				}
			}
			return results;
		}

		private IDownloader GetDownloader(JObject jobject)
		{
			if (jobject == null)
			{
				return new HttpClientDownloader();
			}

			IDownloader downloader;

			var downloaderType = jobject.SelectToken("$.Type")?.ToString();

			switch (downloaderType)
			{
				case "JsEngine":
					{
#if !NET_CORE
						var webDriverDownloader = new WebDriverDownloader(Browser.Chrome);
						downloader = webDriverDownloader;
						break;
#else
						throw new SpiderException("UNSPORT WEBDRIVER DOWNLOADER.");
#endif
					}
				case "Http":
					{
						downloader = new HttpClientDownloader();
						break;
					}
				default:
					{
						downloader = new HttpClientDownloader();
						break;
					}
			}
			return downloader;
		}

		protected override EntitySpider GetEntitySpider()
		{
			EntitySpider context = new EntitySpider(Site)
			{
				CachedSize = CachedSize,
				Deep = Deep,
				Downloader = GetDownloader(Downloader),
				EmptySleepTime = EmptySleepTime,
				SkipWhenResultIsEmpty = SkipWhenResultIsEmpty,
				Scheduler = GetScheduler(Scheduler),
				ThreadNum = ThreadNum,
				Entities = Entities
			};
			SetInfo(Name, (Batch)Enum.Parse(typeof(Batch), Batch));
			context.AddPipelines(GetPipepines(Pipelines));
			context.RedisConnectString = RedisConnectString;
			ConnectString = ConnectString;

			foreach (var entity in Entities)
			{
				EntityProcessor processor = new EntityProcessor(Site, entity);
				context.AddPageProcessor(processor);
			}
			return context;
		}
	}
}
