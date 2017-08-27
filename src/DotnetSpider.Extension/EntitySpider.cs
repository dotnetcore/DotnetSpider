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
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extension.Pipeline;
using NLog;
using DotnetSpider.Core.Pipeline;

namespace DotnetSpider.Extension
{
	public abstract class EntitySpider : CommonSpider
	{
		private static readonly List<string> DefaultProperties = new List<string> { "cdate", Core.Environment.IdColumn };

		public List<Entity> Entities { get; internal set; } = new List<Entity>();

		protected EntitySpider(string name) : this(name, new Site())
		{
		}

		protected EntitySpider(string name, Site site) : base(name, site)
		{
			Core.Infrastructure.Database.DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySql.Data.MySqlClient.MySqlClientFactory.Instance);
		}

		public EntitySpider AddEntityType(Type type, string tableName = null)
		{
			AddEntityType(type, null, tableName);
			return this;
		}

		public EntitySpider AddEntityType<T>(string tableName = null)
		{
			AddEntityType(typeof(T), null, tableName);
			return this;
		}

		public EntitySpider AddEntityType<T>(DataHandler dataHandler)
		{
			AddEntityType(typeof(T), dataHandler);
			return this;
		}

		public EntitySpider AddEntityType(Type type, DataHandler dataHandler, string tableName = null)
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
				if (entity.Table != null && !string.IsNullOrEmpty(tableName))
				{
					entity.Table.Name = tableName;
				}
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
				entityMetadata.Take = entitySelector.Take;
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
				throw new SpiderException("cdate is not available because it's a default property.");
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

		public static string GenerateTableName(string name, TableSuffix suffix)
		{
			switch (suffix)
			{
				case TableSuffix.FirstDayOfCurrentMonth:
					{
						return name + "_" + DateTimeUtils.FirstDayOfCurrentMonth.ToString("yyyy_MM_dd");
					}
				case TableSuffix.Monday:
					{
						return name + "_" + DateTimeUtils.MondayOfCurrentWeek.ToString("yyyy_MM_dd");
					}
				case TableSuffix.Today:
					{
						return name + "_" + DateTime.Now.ToString("yyyy_MM_dd");
					}
			}
			return name;
		}

		protected override IPipeline GetDefaultPipeline()
		{
			IPipeline pipeline;
			switch (Core.Environment.DataConnectionStringSettings.ProviderName)
			{
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
				default:
					{
						pipeline = new MySqlEntityPipeline();
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
	}
}
