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
using DotnetSpider.Extension.Redial;
using DotnetSpider.Core.Infrastructure;
using System.Threading;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Extension
{
	public class EntitySpider : Spider
	{
		private const string InitStatusSetKey = "dotnetspider:init-stats";
		private const string ValidateStatusKey = "dotnetspider:validate-stats";
		private IRedialExecutor _redialExecutor;
		private static readonly List<string> DefaultProperties = new List<string> { "cdate", "__id" };

		[JsonIgnore]
		public Action VerifyCollectedData { get; set; }

		public string RedisConnectString { get; set; }

		[JsonIgnore]
		public RedisConnection RedisConnection { get; private set; }

		public List<Entity> Entities { get; internal set; } = new List<Entity>();

		public PrepareStartUrls[] PrepareStartUrls { get; set; }

		public IRedialExecutor RedialExecutor
		{
			get => _redialExecutor;
			set
			{
				CheckIfRunning();
				_redialExecutor = value;
				NetworkCenter.Current.Executor = RedialExecutor;
			}
		}

		public EntitySpider()
		{
		}

		public EntitySpider(Site site)
		{
			Site = site;
		}

		protected override void PreInitComponent(params string[] arguments)
		{
			if (Site == null)
			{
				throw new SpiderException("Site should not be null.");
			}

			if (Entities == null || Entities.Count == 0)
			{
				throw new SpiderException("Count of entity is zero.");
			}

			foreach (var entity in Entities)
			{
				foreach (var pipeline in Pipelines)
				{
					BaseEntityPipeline newPipeline = pipeline as BaseEntityPipeline;
					newPipeline?.AddEntity(entity);
				}
			}

			bool needInitStartRequest = true;
			var redisConnectString = string.IsNullOrEmpty(RedisConnectString) ? Core.Infrastructure.Configuration.GetValue(Core.Infrastructure.Configuration.RedisConnectString) : RedisConnectString;
			if (!string.IsNullOrEmpty(redisConnectString))
			{
				RedisConnection = Cache.Instance.Get(redisConnectString);
				if (RedisConnection == null)
				{
					RedisConnection = new RedisConnection(redisConnectString);
					Cache.Instance.Set(redisConnectString, RedisConnection);
				}
			}

			if (RedisConnection != null)
			{
				while (!RedisConnection.Database.LockTake(InitLockKey, "0", TimeSpan.FromMinutes(10)))
				{
					Thread.Sleep(1000);
				}
				var lockerValue = RedisConnection.Database.HashGet(InitStatusSetKey, Identity);
				needInitStartRequest = lockerValue != "init finished";
			}

			if (arguments.Contains("rerun"))
			{
				Scheduler.Init(this);
				Scheduler.Clean();
				Scheduler.Dispose();
				RedisConnection?.Database.HashDelete(ValidateStatusKey, Identity);
				needInitStartRequest = true;
			}

			if (needInitStartRequest && PrepareStartUrls != null)
			{
				for (int i = 0; i < PrepareStartUrls.Length; ++i)
				{
					var prepareStartUrl = PrepareStartUrls[i];
					this.Log($"[步骤 {i + 2}] 添加链接到调度中心.", LogLevel.Info);
					prepareStartUrl.Build(this, null);
				}
			}

			RegisterControl(this);

			base.PreInitComponent();
		}

		protected override void AfterInitComponent(params string[] arguments)
		{
			RedisConnection?.Database.LockRelease(InitLockKey, 0);
			base.AfterInitComponent(arguments);
		}

		protected string InitLockKey => $"dotnetspider:initLocker:{Identity}";

		public EntitySpider AddEntityType(Type type)
		{
			AddEntityType(type, null);
			return this;
		}

		public EntitySpider AddEntityType(Type type, DataHandler dataHandler)
		{
			CheckIfRunning();

			if (typeof(SpiderEntity).IsAssignableFrom(type))
			{
				var entity = GenerateEntityMetaData(type.GetTypeInfoCrossPlatform());

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

		public ISpider ToDefaultSpider()
		{
			return new DefaultSpider("", new Site());
		}

		private void HandleVerifyCollectData()
		{
			if (VerifyCollectedData == null)
			{
				return;
			}
			string key = $"dotnetspider:validateLocker:{Identity}";

			try
			{
				bool needInitStartRequest = true;
				if (RedisConnection != null)
				{
					while (!RedisConnection.Database.LockTake(key, "0", TimeSpan.FromMinutes(10)))
					{
						Thread.Sleep(1000);
					}

					var lockerValue = RedisConnection.Database.HashGet(ValidateStatusKey, Identity);
					needInitStartRequest = lockerValue != "verify finished";
				}
				if (needInitStartRequest)
				{
					this.Log("开始执行数据验证...", LogLevel.Info);
					VerifyCollectedData();
				}
				this.Log("数据验证已完成.", LogLevel.Info);

				if (needInitStartRequest)
				{
					RedisConnection?.Database.HashSet(ValidateStatusKey, Identity, "verify finished");
				}
			}
			catch (Exception e)
			{
				this.Log(e.Message, LogLevel.Error, e);
				//throw;
			}
			finally
			{
				RedisConnection?.Database.LockRelease(key, 0);
			}
		}

		private void RegisterControl(ISpider spider)
		{
			if (RedisConnection != null)
			{
				try
				{
					RedisConnection.Subscriber.Subscribe($"{spider.Identity}", (c, m) =>
					{
						switch (m)
						{
							case "PAUSE":
								{
									spider.Pause();
									break;
								}
							case "CONTINUE":
								{
									spider.Contiune();
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
				catch (Exception e)
				{
					spider.Log("Register contol failed.", LogLevel.Error, e);
				}
			}
		}

		public static Entity GenerateEntityMetaData(
#if !NET_CORE
			Type entityType
#else
			TypeInfo entityType
#endif
		)
		{
			Entity entityMetadata = GenerateEntity(entityType);
			entityMetadata.Table = entityType.GetCustomAttribute<Table>();
			if (entityMetadata.Table != null)
			{
				entityMetadata.Table.Name = GenerateTableName(entityMetadata.Table.Name, entityMetadata.Table.Suffix);
			}
			EntitySelector entitySelector = entityType.GetCustomAttribute<EntitySelector>();
			if (entitySelector != null)
			{
				entityMetadata.Multi = true;
				entityMetadata.Selector = new BaseSelector { Expression = entitySelector.Expression, Type = entitySelector.Type };
			}
			else
			{
				entityMetadata.Multi = false;
			}
			var targetUrlsSelectors = entityType.GetCustomAttributes<TargetUrlsSelector>();
			entityMetadata.TargetUrlsSelectors = targetUrlsSelectors.ToList();
			return entityMetadata;
		}

		public static Entity GenerateEntity(
#if !NET_CORE
			Type entityType
#else
			TypeInfo entityType
#endif
		)
		{
			var typeName = entityType.GetTypeCrossPlatform().FullName;
			Entity entity = new Entity
			{
				Name = typeName
			};
			var properties = entityType.GetProperties();
			if (properties.Any(p => DefaultProperties.Contains(p.Name.ToLower())))
			{
				throw new SpiderException("cdate 是默认属性, 请勿使用。");
			}
			foreach (var propertyInfo in properties)
			{
				var propertySelector = propertyInfo.GetCustomAttribute<PropertyDefine>();

				if (propertySelector != null)
				{
					var type = propertyInfo.PropertyType;

					Field token = new Field
					{
						Multi = typeof(IList).IsAssignableFrom(type),
						Option = propertySelector.Option,
						Selector = new BaseSelector
						{
							Expression = propertySelector.Expression,
							Type = propertySelector.Type,
							Argument = propertySelector.Argument
						},
						NotNull = propertySelector.NotNull,
						IgnoreStore = propertySelector.IgnoreStore,
						Length = propertySelector.Length,
						Name = propertyInfo.Name
					};

					foreach (var formatter in propertyInfo.GetCustomAttributes<Formatter>(true))
					{
						token.Formatters.Add(formatter);
					}

					var targetUrl = propertyInfo.GetCustomAttribute<LinkToNext>();
					if (targetUrl != null)
					{
						targetUrl.PropertyName = token.Name;
						entity.LinkToNexts.Add(targetUrl);
					}

					token.DataType = GetDataType(type.Name);

					if (token.DataType != DataType.Text && propertySelector.Length > 0)
					{
						throw new SpiderException("Only string property can set length.");
					}

					entity.Fields.Add(token);
				}
			}
			return entity;
		}

		private static DataType GetDataType(string name)
		{
			switch (name)
			{
				case "Int32":
					{
						return DataType.Int;
					}
				case "Int64":
					{
						return DataType.Bigint;
					}
				case "Single":
					{
						return DataType.Float;
					}
				case "Double":
					{
						return DataType.Double;
					}
				case "String":
					{
						return DataType.Text;
					}
				case "DateTime":
					{
						return DataType.Time;
					}
			}

			return DataType.Text;
		}

		public static string GenerateTableName(string name, TableSuffix suffix)
		{
			switch (suffix)
			{
				case TableSuffix.FirstDayOfThisMonth:
					{
						return name + "_" + DateTimeUtils.Day1OfThisMonth.ToString("yyyy_MM_dd");
					}
				case TableSuffix.Monday:
					{
						return name + "_" + DateTimeUtils.Day1OfThisWeek.ToString("yyyy_MM_dd");
					}
				case TableSuffix.Today:
					{
						return name + "_" + DateTime.Now.ToString("yyyy_MM_dd");
					}
			}
			return name;
		}
	}
}
