using System.Collections.Generic;
using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using Newtonsoft.Json.Linq;
using Java2Dotnet.Spider.Validation;

namespace Java2Dotnet.Spider.Extension.Configuration.Json
{
	public class JsonSpiderContext
	{
		public string SpiderName { get; set; }
		public string UserId { get; set; }
		public string TaskGroup { get; set; }
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
		public List<JObject> PageHandlers { get; set; }
		public JObject TargetUrlsHandler { get; set; }
		public JObject Validations { get; set; }

		public SpiderContext ToRuntimeContext()
		{
			SpiderContext context = new SpiderContext();
			context.CachedSize = CachedSize;
			context.PageHandlers = GetCustomziePage(PageHandlers);
			context.TargetUrlsHandler = GetCustomizeTargetUrls(TargetUrlsHandler);
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
			context.EnviromentValues = EnviromentValues;
			context.Validations = GetValidations(Validations);
			context.UserId = UserId;
			context.TaskGroup = TaskGroup;
			return context;
		}

		private Validations GetValidations(JObject validations)
		{
			if (validations == null)
			{
				return null;
			}

			Validations result = new Validations();
			var source = validations.SelectToken("$.DataSource");
			if (source == null)
			{
				return null;
			}
			result.Source = source.ToObject<DataSource>();
			result.ConnectString = validations.SelectToken("$.ConnectString")?.ToString();
			result.EmailTo = validations.SelectToken("$.ReportTo")?.ToString();
			result.Corporation = validations.SelectToken("$.Corporation")?.ToString();
			result.EmailPassword = validations.SelectToken("$.EmailPassword")?.ToString();
			var port = validations.SelectToken("$.EmailSmtpPort");
			result.EmailSmtpPort = port == null ? 25 : int.Parse(port.ToString());
			result.EmailSmtpServer = validations.SelectToken("$.EmailSmtpServer")?.ToString();
			result.EmailUser = validations.SelectToken("$.EmailUser")?.ToString();

			if (string.IsNullOrEmpty(result.ConnectString) || string.IsNullOrEmpty(result.EmailTo) || string.IsNullOrEmpty(result.EmailPassword) || string.IsNullOrEmpty(result.EmailSmtpServer) || string.IsNullOrEmpty(result.EmailUser))
			{
				return null;
			}

			foreach (var validation in validations.SelectTokens("$.Rules[*]"))
			{
				var type = validation.SelectToken("$.Type")?.ToObject<Validation.Types>();
				if (type == null)
				{
					continue;
				}
				var arguments = validations.SelectToken("$.Arguments")?.ToString();
				var sql = validations.SelectToken("$.Sql")?.ToString();
				var description = validations.SelectToken("$.Description")?.ToString();
				var level = validations.SelectToken("$.Level")?.ToObject<ValidateLevel>();

				result.Rules.Add(GetValidation(type.Value, arguments, sql, description, level.Value));
			}

			if (result.Rules.Count == 0)
			{
				return null;
			}
			else
			{
				return result;
			}
		}

		private Validation GetValidation(Validation.Types type, string arguments, string sql, string description, ValidateLevel level)
		{
			switch (type)
			{
				case Validation.Types.Equal:
					{
						return new EqualValidation { Arguments = arguments, Sql = sql, Description = description, Level = level };
					}
				case Validation.Types.Range:
					{
						return new RangeValidation { Arguments = arguments, Sql = sql, Description = description, Level = level };
					}
			}

			throw new SpiderExceptoin($"Unsported validation type: {type}");
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
							var generalDbPrepareStartUrls = new DbPrepareStartUrls();
							SetDbPrepareStartUrls(generalDbPrepareStartUrls, jobject);

							list.Add(generalDbPrepareStartUrls);
							break;
						}
					case Configuration.PrepareStartUrls.Types.DbList:
						{
							var generalDbPrepareStartUrls = new DbListPrepareStartUrls();
							SetDbPrepareStartUrls(generalDbPrepareStartUrls, jobject);

							list.Add(generalDbPrepareStartUrls);
							break;
						}
					case Configuration.PrepareStartUrls.Types.CommonDb:
						{
							var generalDbPrepareStartUrls = new DbCommonPrepareStartUrls();
							SetDbPrepareStartUrls(generalDbPrepareStartUrls, jobject);

							list.Add(generalDbPrepareStartUrls);
							break;
						}
					case Configuration.PrepareStartUrls.Types.Cycle:
						{
							list.Add(jobject.ToObject<CyclePrepareStartUrls>());
							break;
						}
					case Configuration.PrepareStartUrls.Types.LinkSpider:
						{
							list.Add(jobject.ToObject<LinkSpiderPrepareStartUrls>());
							break;
						}
				}
			}

			return list;
		}

		private void SetDbPrepareStartUrls(AbstractDbPrepareStartUrls generalDbPrepareStartUrls, JObject jobject)
		{
			generalDbPrepareStartUrls.ConnectString = jobject.SelectToken("$.ConnectString")?.ToString();
			generalDbPrepareStartUrls.Filters = jobject.SelectToken("$.Filters")?.ToObject<List<string>>();
			generalDbPrepareStartUrls.FormateStrings = jobject.SelectToken("$.FormateStrings")?.ToObject<List<string>>();
			var limit = jobject.SelectToken("$.Limit");
			generalDbPrepareStartUrls.Limit = limit?.ToObject<int>() ?? int.MaxValue;
			generalDbPrepareStartUrls.TableName = jobject.SelectToken("$.TableName")?.ToString();
			generalDbPrepareStartUrls.Source = jobject.SelectToken("$.Source").ToObject<DataSource>();
			foreach (var column in jobject.SelectTokens("$.Columns[*]"))
			{
				var c = new AbstractDbPrepareStartUrls.Column()
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
				#if !NET_CORE
				case Configuration.Pipeline.Types.MongoDb:
					{
						return pipeline.ToObject<MongoDbPipeline>();
					}
					#endif
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
						break;
#else
						throw new SpiderExceptoin("UNSPORT WEBDRIVER DOWNLOADER.");
#endif
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

			var validations = jobject.SelectToken("$.DownloadValidations");
			if (validations != null)
			{
				foreach (var validation in validations)
				{
					var downloadValidationType = validation.SelectToken("$.Type")?.ToObject<DownloadValidation.Types>();
					if (downloadValidationType == null)
					{
						throw new SpiderExceptoin("Missing DownloadValidation Type: " + jobject);
					}

					switch (downloadValidationType)
					{
						case DownloadValidation.Types.Contains:
							{
								downloader.DownloadValidations.Add(validation.ToObject<ContainsDownloadValidation>());

								break;
							}
						default:
							{
								throw new SpiderExceptoin("Unspodrt validation type: " + downloadValidationType);
							}
					}
				}
			}

			var generatePostBody = jobject.SelectToken("$.GeneratePostBody")?.ToObject<GeneratePostBody>();
			if (generatePostBody != null)
			{
				downloader.GeneratePostBody = generatePostBody;
			}

			return downloader;
		}

		private TargetUrlsHandler GetCustomizeTargetUrls(JObject jobject)
		{
			if (jobject == null)
			{
				return null;
			}
			var customizeTargetUrlsType = jobject.SelectToken("$.Type")?.ToObject<TargetUrlsHandler.Types>();
			if (customizeTargetUrlsType == null)
			{
				throw new SpiderExceptoin("Missing CustomizeTargetUrls Type: " + jobject);
			}
			switch (customizeTargetUrlsType)
			{
				case Configuration.TargetUrlsHandler.Types.IncreasePageNumber:
					{
						return jobject.ToObject<IncreasePageNumberTargetUrlsHandler>();
					}
				case Configuration.TargetUrlsHandler.Types.IncreasePageNumberWithStopper:
					{
						return jobject.ToObject<IncreasePageNumberWithStopperTargetUrlsHandler>();
					}
			}
			throw new SpiderExceptoin("UNSPORT or JSON string is incorrect: " + jobject);
		}

		private List<PageHandler> GetCustomziePage(List<JObject> jobjects)
		{
			if (jobjects == null)
			{
				return null;
			}
			List<PageHandler> list = new List<PageHandler>();
			foreach (var jobject in jobjects)
			{
				var customizePageType = jobject.SelectToken("$.Type").ToObject<PageHandler.Types>();
				switch (customizePageType)
				{
					case PageHandler.Types.Sub:
						{
							list.Add(jobject.ToObject<SubPageHandler>());
							break;
						}
					case PageHandler.Types.CustomTarget:
						{
							list.Add(jobject.ToObject<CustomTargetHandler>());
							break;
						}
				}
			}
			return list;
			//throw new SpiderExceptoin("UNSPORT or JSON string is incorrect: " + jobject);
		}
	}
}
