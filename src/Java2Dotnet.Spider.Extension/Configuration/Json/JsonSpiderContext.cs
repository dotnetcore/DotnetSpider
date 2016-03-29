using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using Newtonsoft.Json.Linq;

namespace Java2Dotnet.Spider.Extension.Configuration.Json
{
	public class JsonSpiderContext
	{
		public string SpiderName { get; set; }
		public int ThreadNum { get; set; } = 1;
		public int Deep { get; set; } = int.MaxValue;
		public int EmptySleepTime { get; set; } = 15000;
		public int CachedSize { get; set; } = 1;
		public JObject Scheduler { get; set; }
		public JObject Downloader { get; set; }
		public Site Site { get; set; }
		public JObject NetworkValidater { get; set; }
		public JObject Redialer { get; set; }
		public List<JObject> PrepareStartUrls { get; set; }
		public List<EnviromentValue> EnviromentValues { get; set; }
		public Dictionary<string, Dictionary<string, object>> StartUrls { get; set; } = new Dictionary<string, Dictionary<string, object>>();

		public JObject Pipeline { get; set; }
		public List<JObject> Entities { get; set; } = new List<JObject>();
		public string Corporation { get; set; }
		public string ValidationReportTo { get; set; }
		public JObject CustomizePage { get; set; }
		public JObject CustomizeTargetUrls { get; set; }

		public SpiderContext ToRuntimeContext()
		{
			SpiderContext context = new SpiderContext();
			context.CachedSize = CachedSize;
			context.Corporation = Corporation;
			context.CustomizePage = GetCustomziePage(CustomizePage);
			context.CustomizeTargetUrls = GetCustomizeTargetUrls(CustomizeTargetUrls);
			context.Deep = Deep;
			context.Downloader = GetDownloader(Downloader);
			context.EmptySleepTime = EmptySleepTime;
			context.Entities = Entities;
			context.NetworkValidater = GetNetworkValidater(NetworkValidater);
			context.Pipeline = GetPipepine(Pipeline);
			context.PrepareStartUrls = GetPrepareStartUrls(PrepareStartUrls);
			context.Redialer = GetRedialer(Redialer);
			context.Scheduler = GetScheduler(Scheduler);
			context.Site = Site;
			context.StartUrls = StartUrls;
			context.SpiderName = SpiderName;
			context.ThreadNum = ThreadNum;
			context.ValidationReportTo = ValidationReportTo;
			context.EnviromentValues = EnviromentValues;
			return context;
		}

		private NetworkValidater GetNetworkValidater(JObject networkValidater)
		{
			if (networkValidater == null)
			{
				return null;
			}

			var type = networkValidater.SelectToken("$.Type")?.ToObject<NetworkValidater.Types>();
			if (type == null)
			{
				throw new SpiderExceptoin("Missing NetworkValidater type: " + networkValidater);
			}

			switch (type)
			{
				case Configuration.NetworkValidater.Types.Vps:
					{
						return networkValidater.ToObject<VpsNetworkValidater>();
					}
				case Configuration.NetworkValidater.Types.Defalut:
					{
						return new DefaultNetworkValidater();
					}
			}
			throw new SpiderExceptoin("Can't convert NetworkValidater: " + networkValidater);
		}

		private Scheduler GetScheduler(JObject jobject)
		{
			if (jobject == null)
			{
				return new QueueScheduler();
			}

			var schedulerType = jobject.SelectToken("$.Type").ToObject<Scheduler.Types>();

			switch (schedulerType)
			{
				case Configuration.Scheduler.Types.Queue:
					{
						return new QueueScheduler();
					}
				case Configuration.Scheduler.Types.Redis:
					{
						return jobject.ToObject<RedisScheduler>();
					}
			}

			throw new SpiderExceptoin("Can't convert Scheduler: " + jobject);
		}

		private Redialer GetRedialer(JObject redialer)
		{
			if (redialer == null)
			{
				return null;
			}

			var type = redialer.SelectToken("$.Type")?.ToObject<Redialer.Types>();

			if (type == null)
			{
				throw new SpiderExceptoin("Missing redialer type: " + redialer);
			}

			switch (type)
			{
				case Configuration.Redialer.Types.Adsl:
					{
						return redialer.ToObject<AdslRedialer>();
					}
				case Configuration.Redialer.Types.H3C:
					{
#if !NET_CORE
						return redialer.ToObject<H3CRedialer>();
#else
						throw new SpiderExceptoin("UNSPORT H3C ADSL NOW.");
#endif
					}
			}

			return null;
		}

		private List<PrepareStartUrls> GetPrepareStartUrls(List<JObject> jobjects)
		{
			if (jobjects == null || jobjects.Count == 0)
			{
				return null;
			}

			var list = new List<PrepareStartUrls>();
			foreach (var jobject in jobjects)
			{
				var type = jobject.SelectToken("$.Type")?.ToObject<PrepareStartUrls.Types>();

				if (type == null)
				{
					throw new SpiderExceptoin("Missing PrepareStartUrls type: " + jobject);
				}

				switch (type)
				{
					case Configuration.PrepareStartUrls.Types.GeneralDb:
						{
							GeneralDbPrepareStartUrls generalDbPrepareStartUrls = new GeneralDbPrepareStartUrls();
							generalDbPrepareStartUrls.ConnectString = jobject.SelectToken("$.ConnectString")?.ToString();
							generalDbPrepareStartUrls.Filters = jobject.SelectToken("$.Filters")?.ToObject<List<string>>();
							generalDbPrepareStartUrls.FormateStrings = jobject.SelectToken("$.FormateStrings")?.ToObject<List<string>>();
							var limit = jobject.SelectToken("$.Limit");
							generalDbPrepareStartUrls.Limit = limit?.ToObject<int>() ?? int.MaxValue;
							generalDbPrepareStartUrls.TableName = jobject.SelectToken("$.TableName")?.ToString();
							generalDbPrepareStartUrls.Source = jobject.SelectToken("$.Source").ToObject<GeneralDbPrepareStartUrls.DataSource>();
							foreach (var column in jobject.SelectTokens("$.Columns[*]"))
							{
								var c = new GeneralDbPrepareStartUrls.Column()
								{
									Name = column.SelectToken("$.Name").ToString()
								};
								foreach (var format in column.SelectTokens("$.Formatters[*]"))
								{
									var name = format.SelectToken("$.Name").ToString();
									var formatterType = FormatterFactory.GetFormatterType(name);
									c.Formatters.Add((Formatter)format.ToObject(formatterType));
								}
								generalDbPrepareStartUrls.Columns.Add(c);
							}

							list.Add(generalDbPrepareStartUrls);
							break;
						}
					case Configuration.PrepareStartUrls.Types.Cycle:
						{
							list.Add(jobject.ToObject<CyclePrepareStartUrls>());
							break;
						}
				}
			}

			return list;
		}

		private Pipeline GetPipepine(JObject pipeline)
		{
			if (pipeline == null)
			{
				throw new SpiderExceptoin("Missing Pipeline.");
			}

			var pipelineType = pipeline.SelectToken("$.Type")?.ToObject<Pipeline.Types>();

			if (pipelineType == null)
			{
				throw new SpiderExceptoin("Missing PrepareStartUrls type: " + pipeline);
			}

			switch (pipelineType)
			{
				case Configuration.Pipeline.Types.MongoDb:
					{
						return pipeline.ToObject<MongoDbPipeline>();
					}
				case Configuration.Pipeline.Types.MySql:
					{
						return pipeline.ToObject<MysqlPipeline>();
					}
				case Configuration.Pipeline.Types.MySqlFile:
					{
						return pipeline.ToObject<MysqlFilePipeline>();
					}
			}

			throw new SpiderExceptoin("UNSPORT PIPELINE: " + pipeline);
		}

		private Downloader GetDownloader(JObject jobject)
		{
			if (jobject == null)
			{
				return new HttpDownloader();
			}

			Downloader downloader;

			var downloaderType = jobject.SelectToken("$.Type")?.ToObject<Downloader.Types>();
			if (downloaderType == null)
			{
				throw new SpiderExceptoin("Missing Downloader type: " + jobject);
			}

			switch (downloaderType)
			{
				case Configuration.Downloader.Types.WebDriverDownloader:
					{
#if !NET_CORE
						var webDriverDownloader = jobject.ToObject<WebDriverDownloader>();
						var loginType = jobject.SelectToken("$.Login.Type");
						if (loginType != null)
						{
							switch (loginType.ToObject<Loginer.Types>())
							{
								case Loginer.Types.Common:
									{
										var login = jobject.SelectToken("$.Login").ToObject<CommonLoginer>();
										webDriverDownloader.Login = login;
										break;
									}
							}
						}
						downloader = webDriverDownloader;
#else
							throw new SpiderExceptoin("UNSPORT WEBDRIVER DOWNLOADER.");
#endif
						break;
					}
				case Configuration.Downloader.Types.HttpClientDownloader:
					{
						downloader = new HttpDownloader();
						break;
					}
				case Configuration.Downloader.Types.FileDownloader:
					{
						downloader = new FileDownloader();
						break;
					}
				default:
					{
						downloader = new HttpDownloader();
						break;
					}
			}

			var downloadValidationType = jobject.SelectToken("$.DownloadValidation.Type")?.ToObject<DownloadValidation.Types>();
			if (downloadValidationType == null)
			{
				throw new SpiderExceptoin("Missing DownloadValidation Type: " + jobject);
			}

			switch (downloadValidationType)
			{
				case DownloadValidation.Types.Contains:
					{
						var validation = jobject.SelectToken("$.DownloadValidation").ToObject<ContainsDownloadValidation>();
						downloader.DownloadValidation = validation;
						break;
					}
				default:
					{
						throw new SpiderExceptoin("Unspodrt validation type: " + downloadValidationType);
					}
			}

			return downloader;
		}

		private CustomizeTargetUrls GetCustomizeTargetUrls(JObject jobject)
		{
			var customizeTargetUrlsType = jobject.SelectToken("$.Type")?.ToObject<CustomizeTargetUrls.Types>();
			if (customizeTargetUrlsType == null)
			{
				throw new SpiderExceptoin("Missing CustomizeTargetUrls Type: " + jobject);
			}
			switch (customizeTargetUrlsType)
			{
				case Configuration.CustomizeTargetUrls.Types.IncreasePageNumber:
					{
						return jobject.ToObject<IncreasePageNumberCustomizeTargetUrls>();
					}
			}
			throw new SpiderExceptoin("UNSPORT or JSON string is incorrect: " + jobject);
		}

		private CustomizePage GetCustomziePage(JObject jobject)
		{
			var customizePageType = jobject.SelectToken("$.Type").ToObject<CustomizePage.Types>();
			switch (customizePageType)
			{
				case Configuration.CustomizePage.Types.Sub:
					{
						return jobject.ToObject<SubCustomizePage>();

					}
			}
			throw new SpiderExceptoin("UNSPORT or JSON string is incorrect: " + jobject);
		}
	}
}
