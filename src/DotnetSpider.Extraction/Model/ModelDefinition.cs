using DotnetSpider.Extraction.Model.Attribute;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotnetSpider.Extraction.Model
{
	public class ModelDefinition : IModel
	{
		/// <summary>
		/// 数据模型的选择器
		/// </summary>
		public Selector Selector { get; protected set; }

		/// <summary>
		/// 从最终解析到的结果中取前 Take 个实体
		/// </summary>
		public int Take { get; protected set; }

		/// <summary>
		/// 设置 Take 的方向, 默认是从头部取
		/// </summary>
		public bool TakeFromHead { get; protected set; }

		/// <summary>
		/// 爬虫实体对应的数据库表信息
		/// </summary>
		public TableInfo Table { get; protected set; }

		/// <summary>
		/// 爬虫实体定义的数据库列信息
		/// </summary>
		public HashSet<FieldSelector> Fields { get; protected set; }

		/// <summary>
		/// 目标链接的选择器
		/// </summary>
		public IEnumerable<TargetRequestSelector> TargetRequestSelectors { get; protected set; }

		/// <summary>
		/// 共享值的选择器
		/// </summary>
		public IEnumerable<SharedValueSelector> SharedValueSelectors { get; protected set; }

		[JsonIgnore]
		public string Identity { get; protected set; }

		public ModelDefinition(Selector selector, IEnumerable<FieldSelector> fields, TableInfo table,
			TargetRequestSelector targetRequestSelector)
			: this(selector, fields, table, new[] { targetRequestSelector })
		{
		}

		public ModelDefinition(Selector selector, IEnumerable<FieldSelector> fields, TableInfo table = null,
			IEnumerable<TargetRequestSelector> targetRequestSelectors = null,
			IEnumerable<SharedValueSelector> sharedValueSelectors = null, int take = 0, bool takeFromHead = true) : this()
		{
			Selector = selector;
			Table = table;
			if (fields == null)
			{
				throw new ExtractionException($"{nameof(fields)} should not be null.");
			}

			Fields = new HashSet<FieldSelector>(fields);
			if (Fields.Count == 0)
			{
				throw new ExtractionException("Count of fields should large than 0.");
			}

			TargetRequestSelectors = targetRequestSelectors;
			SharedValueSelectors = sharedValueSelectors;
			Take = take;
			TakeFromHead = takeFromHead;
			Identity = Table == null ? Guid.NewGuid().ToString("N") : $"{Table.Database}.{Table.FullName}";
		}

		protected ModelDefinition()
		{
		}
	}

	public class ModelDefinition<T> : ModelDefinition
	{
		public ModelDefinition()
		{
			var type = typeof(T);

			var typeName = type.FullName;
			var name = typeName;

			var tableInfo = type.GetCustomAttributes(typeof(TableInfo), true).FirstOrDefault() as TableInfo;

			if (tableInfo != null)
			{
				if (tableInfo.Indexs != null)
				{
					tableInfo.Indexs = new HashSet<string>(tableInfo.Indexs.Select(i => i.Replace(" ", ""))).ToArray();
				}

				if (tableInfo.Uniques != null)
				{
					tableInfo.Uniques = new HashSet<string>(tableInfo.Uniques.Select(i => i.Replace(" ", ""))).ToArray();
				}
			}
			var entitySelector = type.GetCustomAttributes(typeof(EntitySelector), true).FirstOrDefault() as EntitySelector;
			int take = 0;
			bool takeFromHead = true;
			Selector selector = null;
			if (entitySelector != null)
			{
				take = entitySelector.Take;
				takeFromHead = entitySelector.TakeFromHead;
				selector = new Selector { Expression = entitySelector.Expression, Type = entitySelector.Type };
			}

			var targetUrlsSelectors = type.GetCustomAttributes(typeof(TargetRequestSelector), true).Select(s => (TargetRequestSelector)s).ToList();
			var sharedValueSelectors = type.GetCustomAttributes(typeof(SharedValueSelector), true).Select(e =>
			{
				var p = (SharedValueSelector)e;
				return new SharedValueSelector
				{
					Name = p.Name,
					Expression = p.Expression,
					Type = p.Type
				};
			}).ToList();

			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			var fields = new HashSet<FieldSelector>();
			foreach (var property in properties)
			{
				var field = property.GetCustomAttributes(typeof(FieldSelector), true).FirstOrDefault() as FieldSelector;

				if (field == null)
				{
					continue;
				}

				field.Name = property.Name;
				if (field.DataType == DataType.None)
				{
					field.DataType = ConvertDataType(property.PropertyType);
				}

				field.Formatters = property.GetCustomAttributes(typeof(Formatter.Formatter), true).Select(p => (Formatter.Formatter)p).ToArray();
				fields.Add(field);
			}

			Selector = selector;
			Table = tableInfo;

			Fields = fields;
			TargetRequestSelectors = targetUrlsSelectors;
			SharedValueSelectors = sharedValueSelectors;
			Take = take;
			TakeFromHead = takeFromHead;

			if (Table != null)
			{
				Identity = $"{Table.Database}.{Table.FullName}";
			}
			else
			{
				Identity = type.FullName;
			}

			var columns = fields.Where(c => !c.IgnoreStore).ToList();

			if (columns.Count == 0)
			{
				throw new ArgumentException($"Columns is necessary for {name}");
			}

			if (tableInfo != null)
			{
				if (tableInfo.UpdateColumns != null && tableInfo.UpdateColumns.Length > 0)
				{
					foreach (var column in tableInfo.UpdateColumns)
					{
						if (columns.All(c => c.Name != column))
						{
							throw new ArgumentException("Columns set to update are not a property of your entity");
						}
					}

					if (tableInfo.UpdateColumns.Length == 0)
					{
						throw new ArgumentException("There is no column need update");
					}
				}

				if (tableInfo.Indexs != null && tableInfo.Indexs.Length > 0)
				{
					for (int i = 0; i < tableInfo.Indexs.Length; ++i)
					{
						var items = new HashSet<string>(tableInfo.Indexs[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
							.Select(c => c.Trim()));

						if (items.Count == 0)
						{
							throw new ArgumentException("Index should contain more than a column");
						}

						foreach (var item in items)
						{
							var column = columns.FirstOrDefault(c => c.Name == item);
							if (column == null)
							{
								throw new ArgumentException("Columns set as index are not a property of your entity");
							}

							if (column.DataType == DataType.String && (column.Length <= 0 || column.Length > 256))
							{
								throw new ArgumentException("Column length of index should not large than 256");
							}
						}

						tableInfo.Indexs[i] = string.Join(",", items);
					}
				}

				if (tableInfo.Uniques != null && tableInfo.Uniques.Length > 0)
				{
					for (int i = 0; i < tableInfo.Uniques.Length; ++i)
					{
						var items = new HashSet<string>(tableInfo.Uniques[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
							.Select(c => c.Trim()));

						if (items.Count == 0)
						{
							throw new ArgumentException("Unique should contain more than a column");
						}

						foreach (var item in items)
						{
							var column = columns.FirstOrDefault(c => c.Name == item);
							if (column == null)
							{
								throw new ArgumentException("Columns set as unique are not a property of your entity");
							}

							if (column.DataType == DataType.String && (column.Length <= 0 || column.Length > 256))
							{
								throw new ArgumentException("Column length of unique should not large than 256");
							}
						}

						tableInfo.Uniques[i] = string.Join(",", items);
					}
				}
			}
		}

		private DataType ConvertDataType(Type propertyType)
		{
			switch (propertyType.Name)
			{
				case "Int32":
					{
						return DataType.Int;
					}
				case "Decimal":
					{
						return DataType.Decimal;
					}
				case "Single":
					{
						return DataType.Float;
					}
				case "Double":
					{
						return DataType.Double;
					}
				case "Int64":
					{
						return DataType.Long;
					}
				case "Boolean":
					{
						return DataType.Bool;
					}
				case "DateTime":
					{
						return DataType.DateTime;
					}
				default:
					{
						return DataType.String;
					}
			}
		}
	}
}