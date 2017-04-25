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
using DotnetSpider.Core.Pipeline;
#if NET_CORE
#endif

namespace DotnetSpider.Extension
{
	public class EntitySpider : Spider
	{
		private const string InitStatusSetKey = "dotnetspider:init-stats";
		private const string ValidateStatusKey = "dotnetspider:validate-stats";
		private IRedialExecutor _redialExecutor;

		[JsonIgnore]
		public Action VerifyCollectedData { get; set; }
		[JsonIgnore]
		public RedisConnection RedisConnection { get; private set; }

		public List<EntityMetadata> Entities { get; internal set; } = new List<EntityMetadata>();

		public PrepareStartUrls[] PrepareStartUrls { get; set; }

		public IRedialExecutor RedialExecutor
		{
			get
			{
				return _redialExecutor;
			}
			set
			{
				CheckIfRunning();
				_redialExecutor = value;
				NetworkCenter.Current.Executor = RedialExecutor;
			}
		}

		public EntitySpider(Site site) : base()
		{
			if (site == null)
			{
				throw new SpiderException("Site should not be null.");
			}
			Site = site;
		}

		protected override void PreInitComponent(params string[] arguments)
		{
			if (Entities == null || Entities.Count == 0)
			{
				throw new SpiderException("Count of entity is zero.");
			}

			foreach (var entity in Entities)
			{
				foreach (var pipeline in Pipelines)
				{
					BaseEntityPipeline newPipeline = pipeline as BaseEntityPipeline;
					if (newPipeline != null)
					{
						newPipeline.AddEntity(entity);
					}
				}
			}

			bool needInitStartRequest = true;
			var redisConnectString = Configuration.GetValue("redis.connectString");
			if (!string.IsNullOrEmpty(redisConnectString))
			{
				RedisConnection = Cache.Instance.Get(redisConnectString);
				if (RedisConnection == null)
				{
					RedisConnection = new Infrastructure.RedisConnection(redisConnectString);
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
				Scheduler.Dispose();
				if (RedisConnection != null)
				{
					RedisConnection.Database.HashDelete(ValidateStatusKey, Identity);
				}
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
			if (RedisConnection != null)
			{
				RedisConnection.Database.LockRelease(InitLockKey, 0);
			}
			base.AfterInitComponent(arguments);
		}

		protected string InitLockKey
		{
			get
			{
				return $"dotnetspider:initLocker:{Identity}";
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

				if (needInitStartRequest && RedisConnection != null)
				{
					RedisConnection.Database.HashSet(ValidateStatusKey, Identity, "verify finished");
				}
			}
			catch (Exception e)
			{
				this.Log(e.Message, LogLevel.Error, e);
				//throw;
			}
			finally
			{
				if (RedisConnection != null)
				{
					RedisConnection.Database.LockRelease(key, 0);
				}
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

		public static EntityMetadata GenerateEntityMetaData(
#if !NET_CORE
			Type entityType
#else
			TypeInfo entityType
#endif
		)
		{
			EntityMetadata entityMetadata = new EntityMetadata();

			entityMetadata.Table = entityType.GetCustomAttribute<Table>();
			if (entityMetadata.Table != null)
			{
				entityMetadata.Table.Name = GenerateTableName(entityMetadata.Table.Name, entityMetadata.Table.Suffix);
			}
			entityMetadata.Entity = GenerateEntity(entityType);
			EntitySelector entitySelector = entityType.GetCustomAttribute<EntitySelector>();
			if (entitySelector != null)
			{
				entityMetadata.Entity.Multi = true;
				entityMetadata.Entity.Selector = new BaseSelector { Expression = entitySelector.Expression, Type = entitySelector.Type };
			}
			else
			{
				entityMetadata.Entity.Multi = false;
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
			foreach (var propertyInfo in properties)
			{
				var type = propertyInfo.PropertyType;

				if (typeof(ISpiderEntity).IsAssignableFrom(type) || typeof(List<ISpiderEntity>).IsAssignableFrom(type))
				{
					var token = GenerateEntity(propertyInfo.PropertyType.GetTypeInfoCrossPlatform());
					token.Name = propertyInfo.Name;
					token.Multi = typeof(IEnumerable).IsAssignableFrom(type);
					token.Fields.Add(token);
				}
				else
				{
					Field token = new Field();

					var propertySelector = propertyInfo.GetCustomAttribute<PropertyDefine>();
					token.Multi = typeof(IList).IsAssignableFrom(type);

					if (propertySelector != null)
					{
						token.Option = propertySelector.Option;
						token.Selector = new BaseSelector()
						{
							Expression = propertySelector.Expression,
							Type = propertySelector.Type,
							Argument = propertySelector.Argument
						};
						token.NotNull = propertySelector.NotNull;
						token.Store = propertySelector.Store;
						token.Length = propertySelector.Length;

						token.Name = propertyInfo.Name;

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
			}
			return entity;
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
