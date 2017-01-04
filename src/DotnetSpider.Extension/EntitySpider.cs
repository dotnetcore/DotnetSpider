using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.ORM;
using Newtonsoft.Json;
using DotnetSpider.Redial;
using StackExchange.Redis;
using DotnetSpider.Core.Common;
using System.Net;
using DotnetSpider.Core.Monitor;
using System.Threading;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Extension.Downloader;
using LogLevel = DotnetSpider.Core.LogLevel;
#if NET_CORE
using System.Runtime.InteropServices;
#endif

namespace DotnetSpider.Extension
{
	public class EntitySpider : Spider
	{
		private const string InitStatusSetName = "init-status";
		private const string ValidateStatusName = "validate-status";

		protected static ConnectionMultiplexer Redis;
		protected static IDatabase Db;

		protected static string RedisHost { get; set; }
		protected static string RedisPassword { get; set; }
		protected static int RedisPort { get; set; } = 6379;

		[JsonIgnore]
		public Action TaskFinished { get; set; } = () => { };
		[JsonIgnore]
		public Action VerifyCollectedData { get; set; }
		public List<EntityMetadata> Entities { get; internal set; } = new List<EntityMetadata>();
		public RedialExecutor RedialExecutor { get; set; }
		public PrepareStartUrls[] PrepareStartUrls { get; set; }
		public CookieInterceptor CookieInterceptor { get; set; }
		public List<BaseEntityPipeline> EntityPipelines { get; internal set; } = new List<BaseEntityPipeline>();
		public int CachedSize { get; set; }

		static EntitySpider()
		{
			if (string.IsNullOrEmpty(RedisHost))
			{
				RedisHost = Configuration.GetValue("redis.host");
				RedisPassword = Configuration.GetValue("redis.password");
				int port;
				RedisPort = int.TryParse(Configuration.GetValue("redis.port"), out port) ? port : 6379;
			}

			if (!string.IsNullOrEmpty(RedisHost))
			{
				var confiruation = new ConfigurationOptions()
				{
					ServiceName = "DotnetSpider",
					Password = RedisPassword,
					ConnectTimeout = 65530,
					KeepAlive = 8,
					ConnectRetry = 3,
					ResponseTimeout = 3000
				};
#if NET_CORE
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					// Lewis: This is a Workaround for .NET CORE can't use EndPoint to create Socket.
					var address = Dns.GetHostAddressesAsync(RedisHost).Result.FirstOrDefault();
					if (address == null)
					{
						throw new SpiderException($"Can't resovle host: {RedisHost}");
					}
					confiruation.EndPoints.Add(new IPEndPoint(address, RedisPort));
				}
				else
				{
					confiruation.EndPoints.Add(new DnsEndPoint(RedisHost, RedisPort));
				}
#else
				confiruation.EndPoints.Add(new DnsEndPoint(RedisHost, RedisPort));
#endif
				Redis = ConnectionMultiplexer.Connect(confiruation);
				Db = Redis.GetDatabase(1);
			}
		}

		public EntitySpider(Site site)
		{
			if (site == null)
			{
				throw new SpiderException("Site should not be null.");
			}
			Site = site;
		}

		public override void Run(params string[] arguments)
		{
			InitEnvorimentAndVerify();

			try
			{
#if !NET_CORE
				if (CookieInterceptor != null)
				{
					this.Log("尝试获取 Cookie...", LogLevel.Info);
					var cookie = NetworkCenter.Current.Execute("getcookie", () => CookieInterceptor.GetCookie());
					if (cookie == null)
					{
						this.Log("获取 Cookie 失败, 爬虫无法继续.", LogLevel.Warn);
						return;
					}
					else
					{
						Site.CookiesStringPart = cookie.CookiesStringPart;
						Site.SetCookies(cookie.CookiesDictionary);
					}
				}
#endif

				this.Log("创建爬虫...", LogLevel.Info);

				foreach (var entity in Entities)
				{
					string entiyName = entity.Entity.Name;
					var pipelines = new List<BaseEntityPipeline>();
					foreach (var pipeline in EntityPipelines)
					{
						var newPipeline = pipeline.Clone();
						newPipeline.InitiEntity(entity);
						if (newPipeline.IsEnabled)
						{
							pipelines.Add(newPipeline);
						}
					}
					if (pipelines.Count > 0)
					{
						AddPipeline(new EntityPipeline(entiyName, pipelines));
					}
				}

				CheckIfSettingsCorrect();

				bool needInitStartRequest = true;
				string key = "locker-" + Identity;
				if (Db != null)
				{
					while (!Db.LockTake(key, "0", TimeSpan.FromMinutes(10)))
					{
						Thread.Sleep(1000);
					}
					var lockerValue = Db.HashGet(InitStatusSetName, Identity);
					needInitStartRequest = lockerValue != "init finished";
				}

				if (arguments.Contains("rerun"))
				{
					Scheduler.Init(this);
					Scheduler.Clear();
					//DELETE verify record.
					Db?.HashDelete(ValidateStatusName, Identity);
					needInitStartRequest = true;
				}

				this.Log("构建内部模块、准备爬虫数据...", LogLevel.Info);
				InitComponent();

				if (needInitStartRequest)
				{
					if (PrepareStartUrls != null)
					{
						for (int i = 0; i < PrepareStartUrls.Length; ++i)
						{
							var prepareStartUrl = PrepareStartUrls[i];
							this.Log($"[步骤 {i + 2}] 添加链接到调度中心.", LogLevel.Info);
							prepareStartUrl.Build(this, null);
						}
					}
				}

				MonitorCenter.Register(this);

				Db?.LockRelease(key, 0);

				RegisterControl(this);

				if (!arguments.Contains("running-test"))
				{
					base.Run();
				}
				else
				{
					IsExited = true;
				}

				TaskFinished();

				HandleVerifyCollectData();
			}
			finally
			{
				Dispose();
				MonitorCenter.Dispose();
			}
		}

		public EntitySpider AddEntityType(Type type)
		{
			AddEntityType(type, null);
			return this;
		}

		public EntitySpider AddEntityType(Type type, DataHandler dataHandler)
		{
			CheckIfRunning();

			if (typeof(ISpiderEntity).IsAssignableFrom(type))
			{
				var entity = ParseEntityMetaData(type.GetTypeInfoCrossPlatform());

				entity.DataHandler = dataHandler;

				entity.SharedValues = type.GetTypeInfo().GetCustomAttributes<SharedValueSelector>().Select(e => new SharedValueSelector
				{
					Name = e.Name,
					Expression = e.Expression,
					Type = e.Type
				}).ToList();
				Entities.Add(entity);
				EntityProcessor processor = new EntityProcessor(Site, entity);
				AddPageProcessor(processor);
			}
			else
			{
				throw new SpiderException($"Type: {type.FullName} is not a ISpiderEntity.");
			}

			return this;
		}

		public EntitySpider AddEntityPipeline(BaseEntityPipeline pipeline)
		{
			CheckIfRunning();
			EntityPipelines.Add(pipeline);
			return this;
		}

		public void SetCachedSize(int count)
		{
			CheckIfRunning();
			foreach (var pipeline in Pipelines)
			{
				((CachedPipeline)pipeline).CachedSize = count;
			}
		}

		public ISpider ToDefaultSpider()
		{
			return new DefaultSpider("", new Site());
		}

		private static string GetEntityName(Type type)
		{
			return type.FullName;
		}

		private void HandleVerifyCollectData()
		{
			if (VerifyCollectedData == null)
			{
				return;
			}
			string key = "locker-validate-" + Identity;

			try
			{
				bool needInitStartRequest = true;
				if (Redis != null)
				{
					while (!Db.LockTake(key, "0", TimeSpan.FromMinutes(10)))
					{
						Thread.Sleep(1000);
					}

					var lockerValue = Db.HashGet(ValidateStatusName, Identity);
					needInitStartRequest = lockerValue != "verify finished";
				}
				if (needInitStartRequest)
				{
					this.Log("开始执行数据验证...", LogLevel.Info);
					VerifyCollectedData();
				}
				this.Log("数据验证已完成.", LogLevel.Info);

				if (needInitStartRequest && Redis != null)
				{
					Db.HashSet(ValidateStatusName, Identity, "verify finished");
				}
			}
			catch (Exception e)
			{
				this.Log(e.Message, LogLevel.Error, e);
				//throw;
			}
			finally
			{
				if (Redis != null)
				{
					Db.LockRelease(key, 0);
				}
			}
		}

		private void RegisterControl(Spider spider)
		{
			if (Redis != null)
			{
				try
				{
					Redis.GetSubscriber().Subscribe($"{spider.Identity}", (c, m) =>
					{
						switch (m)
						{
							case "STOP":
								{
									spider.Stop();
									break;
								}
							case "RUNASYNC":
								{
									spider.RunAsync();
									break;
								}
							case "EXIT":
								{
									spider.Exit();
									break;
								}
						}
					});
				}
				catch
				{
					// ignored
				}
			}
		}

		private void InitEnvorimentAndVerify()
		{
			if (Entities == null || Entities.Count == 0)
			{
				this.Log("Count of entity is 0.", LogLevel.Error);
				throw new SpiderException("Count of entity is 0.");
			}

			if (EntityPipelines == null || EntityPipelines.Count == 0)
			{
				this.Log("Need at least one entity pipeline.", LogLevel.Error);
				throw new SpiderException("Need at least one entity pipeline.");
			}

			if (RedialExecutor != null)
			{
				RedialExecutor.Init();
				NetworkCenter.Current.Executor = RedialExecutor;
			}
		}

		public static EntityMetadata ParseEntityMetaData(
#if !NET_CORE
			Type entityType
#else
			TypeInfo entityType
#endif
		)
		{
			EntityMetadata entityMetadata = new EntityMetadata();

			entityMetadata.Schema = entityType.GetCustomAttribute<Schema>();
			var indexes = entityType.GetCustomAttribute<Indexes>();
			if (indexes != null)
			{
				entityMetadata.Indexes = indexes.Index?.Select(i => i.Split(',')).ToList();
				entityMetadata.Uniques = indexes.Unique?.Select(i => i.Split(',')).ToList();
				entityMetadata.Primary = indexes.Primary == null ? new List<string>() : indexes.Primary.Split(',').ToList();
				entityMetadata.AutoIncrement = indexes.AutoIncrement == null ? new List<string>() : indexes.AutoIncrement.ToList();
			}
			var updates = entityType.GetCustomAttribute<UpdateColumns>();
			if (updates != null)
			{
				entityMetadata.Updates = updates.Columns.ToList();
			}

			Entity entity = ParseEntity(entityType);
			entityMetadata.Entity = entity;
			EntitySelector extractByAttribute = entityType.GetCustomAttribute<EntitySelector>();
			if (extractByAttribute != null)
			{
				entityMetadata.Entity.Multi = true;
				entityMetadata.Entity.Selector = new BaseSelector { Expression = extractByAttribute.Expression, Type = extractByAttribute.Type };
			}
			else
			{
				entityMetadata.Entity.Multi = false;
			}
			TargetUrlsSelector targetUrlsSelector = entityType.GetCustomAttribute<TargetUrlsSelector>();
			entityMetadata.TargetUrlExtractor = targetUrlsSelector;
			return entityMetadata;
		}

		public static Entity ParseEntity(
#if !NET_CORE
			Type entityType
#else
			TypeInfo entityType
#endif
		)
		{
			Entity entity = new Entity
			{
				Name = GetEntityName(entityType.GetTypeCrossPlatform())
			};
			var properties = entityType.GetProperties();
			foreach (var propertyInfo in properties)
			{
				var type = propertyInfo.PropertyType;

				if (typeof(ISpiderEntity).IsAssignableFrom(type) || typeof(List<ISpiderEntity>).IsAssignableFrom(type))
				{
					Entity token = new Entity();
					if (typeof(IEnumerable).IsAssignableFrom(type))
					{
						token.Multi = true;
					}
					else
					{
						token.Multi = false;
					}
					token.Name = GetEntityName(propertyInfo.PropertyType);
					EntitySelector extractByAttribute = entityType.GetCustomAttribute<EntitySelector>();
					if (extractByAttribute != null)
					{
						token.Selector = new BaseSelector { Expression = extractByAttribute.Expression, Type = extractByAttribute.Type };
					}
					var extractBy = propertyInfo.GetCustomAttribute<PropertySelector>();
					if (extractBy != null)
					{
						token.Selector = new BaseSelector()
						{
							Expression = extractBy.Expression,
							Type = extractBy.Type,
							Argument = extractBy.Argument
						};
						token.NotNull = extractBy.NotNull;
					}

					var targetUrl = propertyInfo.GetCustomAttribute<TargetUrl>();
					if (targetUrl != null)
					{
						throw new SpiderException("Can not set TargetUrl attribute to a ISpiderEntity property.");
					}

					token.Fields.Add(ParseEntity(propertyInfo.PropertyType.GetTypeInfoCrossPlatform()));
				}
				else
				{
					Field token = new Field();

					var extractBy = propertyInfo.GetCustomAttribute<PropertySelector>();
					var storeAs = propertyInfo.GetCustomAttribute<StoredAs>();

					if (typeof(IList).IsAssignableFrom(type))
					{
						token.Multi = true;
					}
					else
					{
						token.Multi = false;
					}

					if (extractBy != null)
					{
						token.Option = extractBy.Option;
						token.Selector = new BaseSelector()
						{
							Expression = extractBy.Expression,
							Type = extractBy.Type,
							Argument = extractBy.Argument
						};
						token.NotNull = extractBy.NotNull;
					}

					if (storeAs != null)
					{
						token.Name = storeAs.Name;
						token.DataType = ParseDataType(storeAs);
					}
					else
					{
						token.Name = propertyInfo.Name;
					}

					foreach (var formatter in propertyInfo.GetCustomAttributes<Formatter>(true))
					{
						token.Formatters.Add(formatter);
					}

					var targetUrl = propertyInfo.GetCustomAttribute<TargetUrl>();
					if (targetUrl != null)
					{
						targetUrl.PropertyName = token.Name;
						entity.TargetUrls.Add(targetUrl);
					}

					entity.Fields.Add(token);
				}
			}
			return entity;
		}

		private static string ParseDataType(StoredAs storedAs)
		{
			string reslut = "";

			switch (storedAs.Type)
			{
				case DataType.Bool:
					{
						reslut = "BOOL";
						break;
					}
				case DataType.Date:
					{
						reslut = "DATE";
						break;
					}
				case DataType.Time:
					{
						reslut = "TIME";
						break;
					}
				case DataType.Text:
					{
						reslut = "TEXT";
						break;
					}

				case DataType.String:
					{
						reslut = $"STRING,{storedAs.Lenth}";
						break;
					}
			}

			return reslut;
		}
	}
}
