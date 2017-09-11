using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotnetSpider.Core;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using Dapper;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extension.Pipeline;
using NLog;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Infrastructure.Database;

namespace DotnetSpider.Extension
{
	public abstract class EntitySpider : CommonSpider
	{
		public List<EntityDefine> Entities { get; internal set; } = new List<EntityDefine>();

		protected EntitySpider(string name) : this(name, new Site())
		{
		}

		protected EntitySpider(string name, Site site) : base(name, site)
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
			return AddEntityType(type, dataHandler);
		}

		public EntityDefine AddEntityType<T>(DataHandler dataHandler)
		{
			return AddEntityType(typeof(T), dataHandler);
		}

		public EntityDefine AddEntityType(Type type, DataHandler dataHandler, string tableName = null)
		{
			CheckIfRunning();

			if (typeof(SpiderEntity).IsAssignableFrom(type))
			{
				var entity = EntityDefine.Parse(type.GetTypeInfoCrossPlatform());

				entity.DataHandler = dataHandler;

				entity.SharedValues = type.GetTypeInfo().GetCustomAttributes<SharedValueSelector>().Select(e => new SharedValueSelector
				{
					Name = e.Name,
					Expression = e.Expression,
					Type = e.Type
				}).ToList();

				if (entity.TableInfo != null && !string.IsNullOrEmpty(tableName))
				{
					entity.TableInfo.Name = tableName;
				}

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
			IPipeline pipeline;
			switch (Core.Environment.DataConnectionStringSettings.ProviderName)
			{
				case "Npgsql":
					{
						pipeline = new PostgreSqlEntityPipeline();
						break;
					}
				case "MySql.Data.MySqlClient":
					{
						pipeline = new MySqlEntityPipeline();
						break;
					}
				case "System.Data.SqlClient":
					{
						pipeline = new SqlServerEntityPipeline();
						break;
					}
				case "MongoDB":
					{
						pipeline = new MongoDbEntityPipeline(Core.Environment.DataConnectionString);
						break;
					}
				default:
					{
						pipeline = new NullPipeline();
						break;
					}
			}
			return pipeline;
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
