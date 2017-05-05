﻿//using System.Collections.Generic;
//using DotnetSpider.Core;
//using DotnetSpider.Extension.Model.Formatter;
//using Newtonsoft.Json.Linq;
//using System.Linq;
//using DotnetSpider.Core.Downloader;
//using DotnetSpider.Core.Scheduler;
//using DotnetSpider.Extension.Scheduler;
//#if !NET_CORE
//using DotnetSpider.Extension.Downloader.WebDriver;
//#endif

//namespace DotnetSpider.Extension.Configuration.Json
//{
//	public class JsonSpiderContext
//	{
//		public string SpiderName { get; set; }
//		public string UserId { get; set; }
//		public string TaskGroup { get; set; }
//		public int ThreadNum { get; set; } = 1;
//		public int Deep { get; set; } = int.MaxValue;
//		public int EmptySleepTime { get; set; } = 15000;
//		public bool SkipWhenResultIsEmpty { get; set; } = false;
//		public int CachedSize { get; set; } = 1;
//		public JObject Scheduler { get; set; }
//		public JObject Downloader { get; set; }
//		public Site Site { get; set; }
//		public JObject RedialExecutor { get; set; }
//		public List<JObject> PrepareStartUrls { get; set; }
//		public List<EnviromentValue> EnviromentValues { get; set; }
//		public Dictionary<string, Dictionary<string, object>> StartUrls { get; set; } = new Dictionary<string, Dictionary<string, object>>();
//		public List<JObject> TargetUrlExtractInfos { get; set; }

//		public List<JObject> Pipelines { get; set; }
//		public List<EntityMetadata> Entities { get; set; } = new List<EntityMetadata>();
//		public List<JObject> PageHandlers { get; set; }
//		public JObject TargetUrlsHandler { get; set; }
//		public JObject Validations { get; set; }

//		public SpiderContext ToRuntimeContext()
//		{
//			SpiderContext context = new SpiderContext
//			{
//				CachedSize = CachedSize,
//				TargetUrlsHandler = GetCustomizeTargetUrls(TargetUrlsHandler),
//				Deep = Deep,
//				Downloader = GetDownloader(Downloader),
//				EmptySleepTime = EmptySleepTime,
//				Entities = Entities,
//				SkipWhenResultIsEmpty = SkipWhenResultIsEmpty,
//				Pipelines = GetPipepines(Pipelines),
//				PrepareStartUrls = GetPrepareStartUrls(PrepareStartUrls),
//				RedialExecutor = GetRedialExecutor(RedialExecutor),
//				Scheduler = GetScheduler(Scheduler),
//				Site = Site,
//				StartUrls = StartUrls,
//				SpiderName = SpiderName,
//				ThreadNum = ThreadNum,
//				EnviromentValues = EnviromentValues,
//				Validations = GetValidations(Validations),
//				UserId = UserId,
//				TaskGroup = TaskGroup,
//				TargetUrlExtractInfos = GetTargetUrlExtractInfos(TargetUrlExtractInfos)
//			};
//			return context;
//		}

//		private List<TargetUrlExtractor> GetTargetUrlExtractInfos(List<JObject> targetUrlExtractInfos)
//		{
//			List<TargetUrlExtractor> list = new List<TargetUrlExtractor>();
//			foreach (var obj in targetUrlExtractInfos)
//			{
//				TargetUrlExtractor t = new TargetUrlExtractor
//				{
//					Patterns = obj.SelectTokens("$.Patterns[*]").Select(p => p.ToString()).ToList(),
//					Region = obj.SelectToken("$.Region").ToObject<Selector>()
//				};
//				foreach (var format in obj.SelectTokens("$.Formatters[*]"))
//				{
//					var name = format.SelectToken("$.Name").ToString();
//					var formatterType = FormatterFactory.GetFormatterType(name);
//					t.Formatters.Add((Formatter)format.ToObject(formatterType));
//				}
//				list.Add(t);
//			}
//			return list;
//		}

//		private Validations GetValidations(JObject validations)
//		{
//			if (validations == null)
//			{
//				return null;
//			}

//			Validations result = new Validations();
//			var source = validations.SelectToken("$.DataSource");
//			if (source == null)
//			{
//				return null;
//			}
//			result.Source = source.ToObject<DataSource>();
//			result.ConnectString = validations.SelectToken("$.ConnectString")?.ToString();
//			result.EmailTo = validations.SelectToken("$.ReportTo")?.ToString();
//			result.Corporation = validations.SelectToken("$.Corporation")?.ToString();
//			result.EmailPassword = validations.SelectToken("$.EmailPassword")?.ToString();
//			var port = validations.SelectToken("$.EmailSmtpPort");
//			result.EmailSmtpPort = port == null ? 25 : int.Parse(port.ToString());
//			result.EmailSmtpServer = validations.SelectToken("$.EmailSmtpServer")?.ToString();
//			result.EmailUser = validations.SelectToken("$.EmailUser")?.ToString();
//			result.EmailFrom = validations.SelectToken("$.EmailFrom")?.ToString();

//			if (string.IsNullOrEmpty(result.ConnectString) || string.IsNullOrEmpty(result.EmailTo) || string.IsNullOrEmpty(result.EmailPassword) || string.IsNullOrEmpty(result.EmailSmtpServer) || string.IsNullOrEmpty(result.EmailUser))
//			{
//				return null;
//			}

//			foreach (var validation in validations.SelectTokens("$.Rules[*]"))
//			{
//				var type = validation.SelectToken("$.Type")?.ToObject<Validation.Types>();
//				if (type == null)
//				{
//					continue;
//				}
//				var arguments = validations.SelectToken("$.Arguments")?.ToString();
//				var sql = validations.SelectToken("$.Sql")?.ToString();
//				var description = validations.SelectToken("$.Description")?.ToString();
//				var level = validations.SelectToken("$.Level")?.ToObject<ValidateLevel>();

//				if (level != null) result.Rules.Add(GetValidation(type.Value, arguments, sql, description, level.Value));
//			}

//			if (result.Rules.Count == 0)
//			{
//				return null;
//			}
//			else
//			{
//				return result;
//			}
//		}

//		private Validation GetValidation(Validation.Types type, string arguments, string sql, string description, ValidateLevel level)
//		{
//			switch (type)
//			{
//				case Validation.Types.Equal:
//					{
//						return new EqualValidation { Arguments = arguments, Sql = sql, Description = description, Level = level };
//					}
//				case Validation.Types.Range:
//					{
//						return new RangeValidation { Arguments = arguments, Sql = sql, Description = description, Level = level };
//					}
//			}

//			throw new SpiderException($"Unsported validation type: {type}");
//		}

//		private InternetDetector GetNetworkValidater(JToken networkValidater)
//		{
//			if (networkValidater == null)
//			{
//				return null;
//			}

//			var type = networkValidater.SelectToken("$.Type")?.ToObject<InternetDetector.Types>();
//			if (type == null)
//			{
//				throw new SpiderException("Missing NetworkValidater type: " + networkValidater);
//			}

//			switch (type)
//			{
//				case InternetDetector.Types.Vps:
//					{
//						return networkValidater.ToObject<VpsInternetDetector>();
//					}
//				case InternetDetector.Types.Defalut:
//					{
//						return new DefaultInternetDetector();
//					}
//#if !NET_CORE
//				case InternetDetector.Types.Vpn:
//					{
//						return new VpnInternetDetector();
//					}
//#endif
//			}
//			throw new SpiderException("Can't convert NetworkValidater: " + networkValidater);
//		}

//		private IScheduler GetScheduler(JObject jobject)
//		{
//			if (jobject == null)
//			{
//				return new QueueDuplicateRemovedScheduler();
//			}

//			var schedulerType = jobject.SelectToken("$.Type").ToObject<Scheduler.Types>();

//			switch (schedulerType)
//			{
//				case Configuration.Scheduler.Types.Queue:
//					{
//						return new QueueScheduler();
//					}
//				case Configuration.Scheduler.Types.Redis:
//					{
//						return jobject.ToObject<RedisScheduler>();
//					}
//			}

//			throw new SpiderException("Can't convert Scheduler: " + jobject);
//		}

//		private RedialExecutor GetRedialExecutor(JObject redialExecutor)
//		{
//			if (redialExecutor == null)
//			{
//				return null;
//			}

//			var type = redialExecutor.SelectToken("$.Type")?.ToObject<RedialExecutor.Types>();

//			if (type == null)
//			{
//				throw new SpiderException("Missing redialer type: " + redialExecutor);
//			}

//			RedialExecutor result ;
//			switch (type)
//			{
//				case Configuration.RedialExecutor.Types.File:
//					{
//						result = redialExecutor.ToObject<FileRedialExecutor>();
//						break;
//					}
//				case Configuration.RedialExecutor.Types.Redis:
//					{
//						result = redialExecutor.ToObject<RedisRedialExecutor>();
//						break;
//					}
//				default:
//					{
//						throw new SpiderException($"Unsport redial executor: {type}");
//					}
//			}
//			result.Redialer = GetRedialer(redialExecutor.SelectToken("$.Redialer"));
//			result.InternetDetector = GetNetworkValidater(redialExecutor.SelectToken("$.NetworkValidater"));
//			return result;
//		}

//		private Redialer GetRedialer(JToken redialer)
//		{
//			if (redialer == null)
//			{
//				return null;
//			}

//			var type = redialer.SelectToken("$.Type")?.ToObject<Redialer.Types>();

//			if (type == null)
//			{
//				throw new SpiderException("Missing redialer type: " + redialer);
//			}

//			Redialer result = null;
//			switch (type)
//			{
//				case Redialer.Types.Adsl:
//					{
//						result = redialer.ToObject<AdslRedialer>();
//						break;
//					}
//				case Redialer.Types.H3C:
//					{
//#if !NET_CORE
//						result = redialer.ToObject<H3CRedialer>();
//						break;
//#else
//						throw new SpiderException("UNSPORT H3C ADSL NOW.");
//#endif
//					}
//				case Redialer.Types.Vpn:
//					{
//#if !NET_CORE
//						result = redialer.ToObject<VpnRedialer>();
//						break;
//#else
//						throw new SpiderException("UNSPORT VPN NOW.");
//#endif
//					}
//			}
//			return result;
//		}

//		private List<PrepareStartUrls> GetPrepareStartUrls(List<JObject> jobjects)
//		{
//			if (jobjects == null || jobjects.Count == 0)
//			{
//				return null;
//			}

//			var list = new List<PrepareStartUrls>();
//			foreach (var jobject in jobjects)
//			{
//				var type = jobject.SelectToken("$.Type")?.ToObject<PrepareStartUrls.Types>();

//				if (type == null)
//				{
//					throw new SpiderException("Missing PrepareStartUrls type: " + jobject);
//				}

//				switch (type)
//				{
//					case Configuration.PrepareStartUrls.Types.ConfigDb:
//						{
//							var generalDbPrepareStartUrls = new ConfigurableDbPrepareStartUrls();
//							SetDbPrepareStartUrls(generalDbPrepareStartUrls, jobject);

//							list.Add(generalDbPrepareStartUrls);
//							break;
//						}
//					case Configuration.PrepareStartUrls.Types.DbList:
//						{
//							var generalDbPrepareStartUrls = new DbListPrepareStartUrls();
//							SetDbPrepareStartUrls(generalDbPrepareStartUrls, jobject);

//							list.Add(generalDbPrepareStartUrls);
//							break;
//						}
//					case Configuration.PrepareStartUrls.Types.CommonDb:
//						{
//							var generalDbPrepareStartUrls = new DbCommonPrepareStartUrls();
//							SetDbPrepareStartUrls(generalDbPrepareStartUrls, jobject);

//							list.Add(generalDbPrepareStartUrls);
//							break;
//						}
//					case Configuration.PrepareStartUrls.Types.Cycle:
//						{
//							list.Add(jobject.ToObject<CyclePrepareStartUrls>());
//							break;
//						}
//					case Configuration.PrepareStartUrls.Types.LinkSpider:
//						{
//							list.Add(jobject.ToObject<LinkSpiderPrepareStartUrls>());
//							break;
//						}
//					case Configuration.PrepareStartUrls.Types.Base:
//						{
//							var generalDbPrepareStartUrls = new BaseDbPrepareStartUrls();
//							foreach (var column in jobject.SelectTokens("$.Columns[*]"))
//							{
//								var c = new BaseDbPrepareStartUrls.Column()
//								{
//									Name = column.SelectToken("$.Name").ToString()
//								};
//								foreach (var format in column.SelectTokens("$.Formatters[*]"))
//								{
//									var name = format.SelectToken("$.Name").ToString();
//									var formatterType = FormatterFactory.GetFormatterType(name);
//									c.Formatters.Add((Formatter)format.ToObject(formatterType));
//								}

//								generalDbPrepareStartUrls.Columns.Add(c);
//							}
//							generalDbPrepareStartUrls.ConnectString = jobject.SelectToken("$.ConnectString").ToString();
//							generalDbPrepareStartUrls.FormateStrings = jobject.SelectToken("$.FormateStrings").ToObject<List<string>>();
//							generalDbPrepareStartUrls.Method = jobject.SelectToken("$.Method").ToString();
//							generalDbPrepareStartUrls.Origin = jobject.SelectToken("$.Origin").ToString();
//							generalDbPrepareStartUrls.PostBody = jobject.SelectToken("$.PostBody").ToString();
//							generalDbPrepareStartUrls.QueryString = jobject.SelectToken("$.QueryString").ToString();
//							generalDbPrepareStartUrls.Referer = jobject.SelectToken("$.Referer").ToString();
//							generalDbPrepareStartUrls.Source = jobject.SelectToken("$.Source").ToObject<DataSource>();
//							generalDbPrepareStartUrls.Extras = jobject.SelectToken("$.Extras").ToObject<Dictionary<string, object>>();
//							list.Add(generalDbPrepareStartUrls);
//							break;
//						}
//				}
//			}

//			return list;
//		}

//		private void SetDbPrepareStartUrls(ConfigurableDbPrepareStartUrls generalDbPrepareStartUrls, JObject jobject)
//		{
//			generalDbPrepareStartUrls.ConnectString = jobject.SelectToken("$.ConnectString")?.ToString();
//			generalDbPrepareStartUrls.Filters = jobject.SelectToken("$.Filters")?.ToObject<List<string>>();
//			generalDbPrepareStartUrls.FormateStrings = jobject.SelectToken("$.FormateStrings")?.ToObject<List<string>>();
//			var limit = jobject.SelectToken("$.Limit");
//			generalDbPrepareStartUrls.Limit = limit?.ToObject<int>() ?? int.MaxValue;
//			generalDbPrepareStartUrls.TableName = jobject.SelectToken("$.TableName")?.ToString();
//			generalDbPrepareStartUrls.Source = jobject.SelectToken("$.Source").ToObject<DataSource>();
//			foreach (var column in jobject.SelectTokens("$.Columns[*]"))
//			{
//				var c = new BaseDbPrepareStartUrls.Column()
//				{
//					Name = column.SelectToken("$.Name").ToString()
//				};
//				foreach (var format in column.SelectTokens("$.Formatters[*]"))
//				{
//					var name = format.SelectToken("$.Name").ToString();
//					var formatterType = FormatterFactory.GetFormatterType(name);
//					c.Formatters.Add((Formatter)format.ToObject(formatterType));
//				}
//				generalDbPrepareStartUrls.Columns.Add(c);
//			}
//		}

//		private List<Pipeline> GetPipepines(List<JObject> pipelines)
//		{
//			if (pipelines == null || pipelines.Count == 0)
//			{
//				throw new SpiderException("Missing Pipeline.");
//			}

//			List<Pipeline> results = new List<Pipeline>();
//			foreach (var pipeline in pipelines)
//			{
//				Pipeline tmp = null;
//				var pipelineType = pipeline.SelectToken("$.Type")?.ToObject<Pipeline.Types>();

//				if (pipelineType == null)
//				{
//					throw new SpiderException("Missing PrepareStartUrls type: " + pipeline);
//				}

//				switch (pipelineType)
//				{
//#if !NET_CORE
//					case Pipeline.Types.MongoDb:
//						{
//							tmp = (pipeline.ToObject<MongoDbPipeline>());
//							break;
//						}
//#endif
//					case Pipeline.Types.MySql:
//						{
//							tmp = (pipeline.ToObject<MysqlPipeline>());
//							break;
//						}
//					case Pipeline.Types.MySqlFile:
//						{
//							tmp = pipeline.ToObject<MysqlFilePipeline>();
//							break;
//						}
//				}

//				if (tmp == null)
//				{
//					throw new SpiderException("UNSPORT PIPELINE: " + pipeline);
//				}
//				else
//				{
//					results.Add(tmp);
//				}
//			}
//			return results;
//		}

//		private Downloader GetDownloader(JObject jobject)
//		{
//			if (jobject == null)
//			{
//				return new HttpDownloader();
//			}

//			Downloader downloader;

//			var downloaderType = jobject.SelectToken("$.Type")?.ToObject<Downloader.Types>();
//			if (downloaderType == null)
//			{
//				throw new SpiderException("Missing Downloader type: " + jobject);
//			}

//			switch (downloaderType)
//			{
//				case Configuration.Downloader.Types.WebDriverDownloader:
//					{
//#if !NET_CORE
//						var webDriverDownloader = new WebDriverDownloader();
//						var loginType = jobject.SelectToken("$.Login.Type");
//						if (loginType != null)
//						{
//							switch (loginType.ToObject<Loginer.Types>())
//							{
//								case Loginer.Types.Common:
//									{
//										var login = jobject.SelectToken("$.Login").ToObject<CommonLoginer>();
//										webDriverDownloader.Login = login;
//										break;
//									}
//								case Loginer.Types.Manual:
//									{
//										webDriverDownloader.Login = jobject.SelectToken("$.Login").ToObject<ManualLoginer>();
//										break;
//									}
//							}
//						}
//						webDriverDownloader.Browser = jobject.SelectToken("$.Browser").ToObject<Browser>();
//						//webDriverDownloader.RedialLimit = jobject.SelectToken("$.RedialLimit").ToObject<int>();
//						webDriverDownloader.PostBodyGenerator = jobject.SelectToken("$.PostBodyGenerator").ToObject<PostBodyGenerator>();
//						webDriverDownloader.VerifyCode = jobject.SelectToken("$.VerifyCode").ToObject<VerifyCode>();

//						downloader = webDriverDownloader;
//						break;
//#else
//						throw new SpiderException("UNSPORT WEBDRIVER DOWNLOADER.");
//#endif
//					}
//				case Configuration.Downloader.Types.HttpClientDownloader:
//					{
//						downloader = new HttpDownloader();
//						break;
//					}
//				case Configuration.Downloader.Types.FileDownloader:
//					{
//						downloader = new FileDownloader();
//						break;
//					}
//				default:
//					{
//						downloader = new HttpDownloader();
//						break;
//					}
//			}

//			downloader.Handlers = GetDownloadHandlers(jobject.SelectTokens("$.Handlers[*]"));

//			var postBodyGenerator = jobject.SelectToken("$.PostBodyGenerator")?.ToObject<PostBodyGenerator>();
//			if (postBodyGenerator != null)
//			{
//				downloader.PostBodyGenerator = postBodyGenerator;
//			}

//			return downloader;
//		}

//		private TargetUrlsHandler GetCustomizeTargetUrls(JObject jobject)
//		{
//			if (jobject == null)
//			{
//				return null;
//			}
//			var customizeTargetUrlsType = jobject.SelectToken("$.Type")?.ToObject<TargetUrlsHandler.Types>();
//			if (customizeTargetUrlsType == null)
//			{
//				throw new SpiderException("Missing CustomizeTargetUrls Type: " + jobject);
//			}
//			switch (customizeTargetUrlsType)
//			{
//				case Configuration.TargetUrlsHandler.Types.IncreasePageNumber:
//					{
//						return jobject.ToObject<IncreasePageNumberTargetUrlsHandler>();
//					}
//				case Configuration.TargetUrlsHandler.Types.IncreasePageNumberWithStopper:
//					{
//						return jobject.ToObject<IncreasePageNumberWithStopperTargetUrlsHandler>();
//					}
//				case Configuration.TargetUrlsHandler.Types.CustomLimitIncreasePageNumber:
//					{
//						return jobject.ToObject<CustomLimitIncreasePageNumberTargetUrlsHandler>();
//					}
//				case Configuration.TargetUrlsHandler.Types.IncreasePageNumberTimeStopper:
//					{
//						return jobject.ToObject<IncreasePageNumbeTimeStopperTargetUrlsHandler>();
//					}
//				case Configuration.TargetUrlsHandler.Types.IncreasePostPageNumber:
//					{
//						return jobject.ToObject<IncreasePostPageNumberTargetUrlsHandler>();
//					}
//				case Configuration.TargetUrlsHandler.Types.IncreasePostPageNumberWithStopper:
//					{
//						return jobject.ToObject<IncreasePostPageNumberWithStopperTargetUrlsHandler>();
//					}
//				case Configuration.TargetUrlsHandler.Types.IncreasePostPageNumberTimeStopper:
//					{
//						return jobject.ToObject<IncreasePostPageNumberTimeStopperTargetUrlsHandler>();
//					}
//			}
//			throw new SpiderException("UNSPORT or JSON string is incorrect: " + jobject);
//		}

//		private List<IDownloadHandler> GetDownloadHandlers(IEnumerable<JToken> jobjects)
//		{
//			if (jobjects == null)
//			{
//				return null;
//			}
//			List<IDownloadHandler> list = new List<IDownloadHandler>();

//			foreach (var handler in jobjects)
//			{
//				var handlerType = handler.SelectToken("$.Type")?.ToObject<DownloadHandler.Types>();
//				if (handlerType == null)
//				{
//					throw new SpiderException("Missing handler Type for " + handler);
//				}

//				switch (handlerType)
//				{
//					case DownloadHandler.Types.RedialWhenContainsIllegalString:
//						{
//							list.Add(handler.ToObject<RedialWhenContainsIllegalStringHandler>());
//							break;
//						}
//					case DownloadHandler.Types.SubContent:
//						{
//							list.Add(handler.ToObject<SubContentHandler>());
//							break;
//						}
//					case DownloadHandler.Types.CustomTarget:
//						{
//							list.Add(handler.ToObject<CustomTargetHandler>());
//							break;
//						}
//					case DownloadHandler.Types.ContentToUpperOrLower:
//						{
//							list.Add(handler.ToObject<ContentToUpperOrLowerHandler>());
//							break;
//						}
//					case DownloadHandler.Types.RedialWhenExceptionThrow:
//						{
//							list.Add(handler.ToObject<RedialWhenExceptionThrowHandler>());
//							break;
//						}
//					case DownloadHandler.Types.RegexMatchContent:
//						{
//							list.Add(handler.ToObject<RegexMatchContentHandler>());
//							break;
//						}
//					case DownloadHandler.Types.RemoveContent:
//						{
//							list.Add(handler.ToObject<RemoveContentHandler>());
//							break;
//						}
//					case DownloadHandler.Types.RemoveContentHtmlTag:
//						{
//							list.Add(handler.ToObject<RemoveContentHtmlTagHandler>());
//							break;
//						}
//					case DownloadHandler.Types.ReplaceContent:
//						{
//							list.Add(handler.ToObject<ReplaceContentHandler>());
//							break;
//						}
//					case DownloadHandler.Types.TrimContent:
//						{
//							list.Add(handler.ToObject<TrimContentHandler>());
//							break;
//						}
//					case DownloadHandler.Types.UnescapeContent:
//						{
//							list.Add(handler.ToObject<UnescapeContentHandler>());
//							break;
//						}
//					default:
//						{
//							throw new SpiderException("Unspodrt handler type: " + handlerType);
//						}
//				}
//			}

//			return list;
//			//throw new SpiderExceptoin("UNSPORT or JSON string is incorrect: " + jobject);
//		}
//	}
//}
