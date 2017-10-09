using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Extension
{
	public abstract class EntitySpider : CommonSpider
	{
		public List<EntityDefine> Entities { get; internal set; } = new List<EntityDefine>();

		public EntitySpider(Site site) : base(site)
		{
		}

		public EntitySpider() : this(new Site())
		{
		}

		public EntitySpider(string name) : base(name)
		{
		}

		public EntitySpider(string name, Site site) : base(name, site)
		{
		}

		public EntityDefine AddEntityType(Type type, string tableName = null)
		{
			return AddEntityType(type, null, tableName);
		}

		public EntityDefine AddEntityType<T>(string tableName = null)
		{
			return AddEntityType(typeof(T), null, tableName);
		}

		public EntityDefine AddEntityType(Type type, DataHandler dataHandler)
		{
			return AddEntityType(type, dataHandler, null);
		}

		public EntityDefine AddEntityType<T>(DataHandler dataHandler)
		{
			return AddEntityType(typeof(T), dataHandler);
		}

		public EntityDefine AddEntityType(Type type, DataHandler dataHandler, string tableName)
		{
			CheckIfRunning();

			if (typeof(SpiderEntity).IsAssignableFrom(type))
			{
				var entity = EntityDefine.Parse(type.GetTypeInfoCrossPlatform());
				if (entity.TableInfo != null && !string.IsNullOrEmpty(tableName))
				{
					entity.TableInfo.Name = tableName;
				}
				entity.DataHandler = dataHandler;

				Entities.Add(entity);
				EntityProcessor processor = new EntityProcessor(Site, entity);
				AddPageProcessor(processor);
				return entity;
			}
			else
			{
				throw new SpiderException($"Type: {type.FullName} is not a SpiderEntity.");
			}
		}

		protected override IPipeline GetDefaultPipeline()
		{
			return BaseEntityDbPipeline.GetPipelineFromAppConfig();
		}

		protected override void PreInitComponent(params string[] arguments)
		{
			base.PreInitComponent(arguments);

			if (arguments.Contains("skip"))
			{
				return;
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

			if (IfRequireInitStartRequests(arguments) && StartUrlBuilders != null && StartUrlBuilders.Count > 0)
			{
				for (int i = 0; i < StartUrlBuilders.Count; ++i)
				{
					var builder = StartUrlBuilders[i];
					Logger.MyLog(Identity, $"[{i + 1}] Add extra start urls to scheduler.", LogLevel.Info);
					builder.Build(Site);
				}
			}
		}
	}
}
