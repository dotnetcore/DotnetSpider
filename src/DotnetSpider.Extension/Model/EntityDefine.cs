using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotnetSpider.Extension.Model
{
	public class EntityDefine : AbstractSelector
	{
		private static readonly List<string> DefaultProperties = new List<string> { "cdate", Core.Environment.IdColumn };

		public EntityTable TableInfo { get; set; }

		public List<Column> Columns { get; set; } = new List<Column>();

		public int Take { get; set; }

		public List<TargetUrlsSelector> TargetUrlsSelectors { get; set; }

		public List<LinkToNext> LinkToNexts { get; set; } = new List<LinkToNext>();

		public DataHandler DataHandler { get; set; }

		public List<SharedValueSelector> SharedValues { get; internal set; } = new List<SharedValueSelector>();

		public static EntityDefine Parse(
#if !NET_CORE
			Type entityType
#else
			TypeInfo entityType
#endif
		)
		{
			EntityDefine entityDefine = new EntityDefine();
			GenerateEntityColumns(entityType, entityDefine);

			entityDefine.TableInfo = entityType.GetCustomAttribute<EntityTable>();

			if (entityDefine.TableInfo != null)
			{
				if (entityDefine.TableInfo.Indexs != null)
				{
					entityDefine.TableInfo.Indexs = new HashSet<string>(entityDefine.TableInfo.Indexs.Select(i => i.Replace(" ", ""))).ToArray();
				}
				if (entityDefine.TableInfo.Uniques != null)
				{
					entityDefine.TableInfo.Uniques = new HashSet<string>(entityDefine.TableInfo.Uniques.Select(i => i.Replace(" ", ""))).ToArray();
				}
			}
			EntitySelector entitySelector = entityType.GetCustomAttribute<EntitySelector>();
			if (entitySelector != null)
			{
				entityDefine.Multi = true;
				entityDefine.Take = entitySelector.Take;
				entityDefine.Selector = new BaseSelector { Expression = entitySelector.Expression, Type = entitySelector.Type };
			}
			else
			{
				entityDefine.Multi = false;
			}
			var targetUrlsSelectors = entityType.GetCustomAttributes<TargetUrlsSelector>();
			entityDefine.TargetUrlsSelectors = targetUrlsSelectors.ToList();

			entityDefine.SharedValues = entityType.GetCustomAttributes<SharedValueSelector>().Select(e => new SharedValueSelector
			{
				Name = e.Name,
				Expression = e.Expression,
				Type = e.Type
			}).ToList();

			ValidateEntityDefine(entityDefine);

			return entityDefine;
		}

		public static EntityDefine Parse<T>()
		{
#if !NET_CORE
			Type type = typeof(T);
#else
			TypeInfo type=typeof(T).GetTypeInfo();
#endif
			return Parse(type);
		}

		public static void CreateTable<T>(BaseEntityDbPipeline pipeline)
		{
			var entity = Parse<T>();
			pipeline.AddEntity(entity);
			pipeline.InitDatabaseAndTable();
		}

		private static void GenerateEntityColumns(
#if !NET_CORE
			Type entityType
#else
			TypeInfo entityType
#endif
			, EntityDefine entity
)
		{
			var typeName = entityType.GetTypeCrossPlatform().FullName;
			entity.Name = typeName;

			var properties = entityType.GetProperties();
			if (properties.Any(p => DefaultProperties.Contains(p.Name.ToLower())))
			{
				throw new SpiderException("cdate is not available because it's a default property.");
			}
			foreach (var propertyInfo in properties)
			{
				var propertySelector = propertyInfo.GetCustomAttribute<PropertyDefine>();
				if (propertySelector == null)
				{
					continue;
				}

				var type = propertyInfo.PropertyType;

				var column = new Column
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

				foreach (var formatter in propertyInfo.GetCustomAttributes<Formatter.Formatter>(true))
				{
					column.Formatters.Add(formatter);
				}

				var targetUrl = propertyInfo.GetCustomAttribute<LinkToNext>();
				if (targetUrl != null)
				{
					targetUrl.PropertyName = column.Name;
					entity.LinkToNexts.Add(targetUrl);
				}

				column.DataType = type.Name;

				if (column.DataType != DataTypeNames.String && propertySelector.Length > 0)
				{
					throw new SpiderException("Only string property can set length.");
				}

				entity.Columns.Add(column);
			}
		}

		private static void ValidateEntityDefine(EntityDefine entity)
		{
			var columns = GetColumns(entity);

			if (columns.Count == 0)
			{
				throw new SpiderException($"Columns is necessary for {entity.Name}.");
			}
			if (entity.TableInfo == null)
			{
				return;
			}
			if (!string.IsNullOrEmpty(entity.TableInfo.Primary))
			{
				if (entity.TableInfo.Primary != Core.Environment.IdColumn)
				{
					var items = new HashSet<string>(entity.TableInfo.Primary.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()));
					if (items.Count > 0)
					{
						foreach (var item in items)
						{
							var column = columns.FirstOrDefault(c => c.Name == item);
							if (column == null)
							{
								throw new SpiderException("Columns set as primary is not a property of your entity.");
							}
							if (column.DataType == DataTypeNames.String && (column.Length > 256 || column.Length <= 0))
							{
								throw new SpiderException("Column length of primary should not large than 256.");
							}
							column.NotNull = true;
						}
					}
					else
					{
						entity.TableInfo.Primary = Core.Environment.IdColumn;
					}
				}
			}
			else
			{
				entity.TableInfo.Primary = Core.Environment.IdColumn;
			}

			if (entity.TableInfo.UpdateColumns != null && entity.TableInfo.UpdateColumns.Length > 0)
			{
				foreach (var column in entity.TableInfo.UpdateColumns)
				{
					if (columns.All(c => c.Name != column))
					{
						throw new SpiderException("Columns set as update is not a property of your entity.");
					}
				}
				var updateColumns = new List<string>(entity.TableInfo.UpdateColumns);
				updateColumns.Remove(entity.TableInfo.Primary);

				entity.TableInfo.UpdateColumns = updateColumns.ToArray();

				if (entity.TableInfo.UpdateColumns.Length == 0)
				{
					throw new SpiderException("There is no column need update.");
				}
			}

			if (entity.TableInfo.Indexs != null && entity.TableInfo.Indexs.Length > 0)
			{
				for (int i = 0; i < entity.TableInfo.Indexs.Length; ++i)
				{
					var items = new HashSet<string>(entity.TableInfo.Indexs[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()));

					if (items.Count == 0)
					{
						throw new SpiderException("Index should contain more than a column.");
					}
					if (items.Count == 1 && items.First() == entity.TableInfo.Primary)
					{
						throw new SpiderException("Primary is no need to create another index.");
					}
					foreach (var item in items)
					{
						var column = columns.FirstOrDefault(c => c.Name == item);
						if (column == null)
						{
							throw new SpiderException("Columns set as index is not a property of your entity.");
						}
						if (column.DataType == DataTypeNames.String && (column.Length <= 0 || column.Length > 256))
						{
							throw new SpiderException("Column length of index should not large than 256.");
						}
					}
					entity.TableInfo.Indexs[i] = string.Join(",", items);
				}
			}
			if (entity.TableInfo.Uniques != null && entity.TableInfo.Uniques.Length > 0)
			{
				for (int i = 0; i < entity.TableInfo.Uniques.Length; ++i)
				{
					var items = new HashSet<string>(entity.TableInfo.Uniques[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()));

					if (items.Count == 0)
					{
						throw new SpiderException("Unique should contain more than a column.");
					}
					if (items.Count == 1 && items.First() == entity.TableInfo.Primary)
					{
						throw new SpiderException("Primary is no need to create another unique.");
					}
					foreach (var item in items)
					{
						var column = columns.FirstOrDefault(c => c.Name == item);
						if (column == null)
						{
							throw new SpiderException("Columns set as unique is not a property of your entity.");
						}
						if (column.DataType == DataTypeNames.String && (column.Length <= 0 || column.Length > 256))
						{
							throw new SpiderException("Column length of unique should not large than 256.");
						}
					}
					entity.TableInfo.Uniques[i] = string.Join(",", items);
				}
			}
		}

		private static List<Column> GetColumns(EntityDefine entity)
		{
			var columns = new List<Column>();
			foreach (var f in entity.Columns)
			{
				var column = f;
				if (!column.IgnoreStore)
				{
					columns.Add(column);
				}
			}
			return columns;
		}
	}

	public class Column : AbstractSelector
	{
		public PropertyDefine.Options Option { get; set; }

		public int Length { get; set; }

		public string DataType { get; set; }

		public bool IgnoreStore { get; set; }

		public List<Formatter.Formatter> Formatters { get; set; } = new List<Formatter.Formatter>();
	}

	public abstract class AbstractSelector
	{
		public BaseSelector Selector { get; set; }

		public bool NotNull { get; set; }

		public bool Multi { get; set; }

		public string Name { get; set; }
	}
}