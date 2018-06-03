using DotnetSpider.Core;
using DotnetSpider.Extension.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotnetSpider.Extension.Model
{
	public class ModelDefine : IModel
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
		public TableInfo TableInfo { get; protected set; }

		/// <summary>
		/// 爬虫实体定义的数据库列信息
		/// </summary>
		public HashSet<Field> Fields { get; protected set; }

		/// <summary>
		/// 目标链接的选择器
		/// </summary>
		public IEnumerable<TargetUrlsSelector> TargetUrlsSelectors { get; protected set; }

		/// <summary>
		/// 共享值的选择器
		/// </summary>
		public IEnumerable<SharedValueSelector> SharedValueSelectors { get; protected set; }

		public string Identity { get; protected set; }

		public ModelDefine(Selector selector, IEnumerable<Field> fields, TableInfo table = null, IEnumerable<TargetUrlsSelector> targetUrlsSelectors = null, IEnumerable<SharedValueSelector> sharedValueSelectors = null, int take = 0, bool takeFromHead = true) : this()
		{
			Selector = selector;
			TableInfo = table;
			if (fields == null || fields.Count() == 0)
			{
				throw new SpiderException("Count of fields should large than 0.");
			}
			Fields = new HashSet<Field>(fields);
			TargetUrlsSelectors = targetUrlsSelectors;
			SharedValueSelectors = sharedValueSelectors;
			Take = take;
			TakeFromHead = takeFromHead;
			Identity = TableInfo == null ? Guid.NewGuid().ToString("N") : $"{TableInfo.Database}.{TableInfo.FullName}";
		}

		protected ModelDefine()
		{
		}
	}

	public class ModelDefine<T> : ModelDefine
	{
		public ModelDefine()
		{
			var type = typeof(T);

			var typeName = type.FullName;
			var Name = typeName;

			var tableInfo = type.GetCustomAttribute<TableInfo>();

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

			EntitySelector entitySelector = type.GetCustomAttribute<EntitySelector>();
			int take = 0;
			bool takeFromHead = true;
			Selector selector = null;
			if (entitySelector != null)
			{
				take = entitySelector.Take;
				takeFromHead = entitySelector.TakeFromHead;
				selector = new Selector { Expression = entitySelector.Expression, Type = entitySelector.Type };
			}

			var targetUrlsSelectors = type.GetCustomAttributes<TargetUrlsSelector>().ToList();
			var sharedValueSelectors = type.GetCustomAttributes<SharedValueSelector>().Select(e => new SharedValueSelector
			{
				Name = e.Name,
				Expression = e.Expression,
				Type = e.Type
			}).ToList();

			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			var fields = new HashSet<Field>();
			foreach (var property in properties)
			{
				var field = property.GetCustomAttribute<Field>();
				if (field == null)
				{
					continue;
				}
				field.Name = property.Name;
				if (field.DataType == DataType.None)
				{
					field.DataType = ConvertDataType(property.PropertyType);
				}
				field.Formatters = property.GetCustomAttributes<Formatter.Formatter>(true).ToArray();
				fields.Add(field);
			}


			Selector = selector;
			TableInfo = tableInfo;

			Fields = fields;
			TargetUrlsSelectors = targetUrlsSelectors;
			SharedValueSelectors = sharedValueSelectors;
			Take = take;
			TakeFromHead = takeFromHead;

			if (TableInfo != null)
			{
				TableInfo.PrimaryKey = TableInfo.PrimaryKey?.Trim().ToLower();
				Fields.RemoveWhere(f => f.Name.ToLower() == TableInfo.PrimaryKey);
				Identity = $"{TableInfo.Database}.{TableInfo.FullName}";
			}
			else
			{
				Identity = type.FullName;
			}

			var columns = fields.Where(c => !c.IgnoreStore).ToList();

			if (columns.Count == 0)
			{
				throw new SpiderException($"Columns is necessary for {Name}");
			}
			if (tableInfo != null)
			{
				if (tableInfo.UpdateColumns != null && tableInfo.UpdateColumns.Length > 0)
				{
					foreach (var column in tableInfo.UpdateColumns)
					{
						if (columns.All(c => c.Name != column) && column.ToLower() != tableInfo.PrimaryKey)
						{
							throw new SpiderException("Columns set to update are not a property of your entity");
						}
					}

					if (tableInfo.UpdateColumns.Length == 0)
					{
						throw new SpiderException("There is no column need update");
					}
					if (tableInfo.UpdateColumns.Any(c => c.ToLower() == tableInfo.PrimaryKey))
					{
						throw new SpiderException("Primary can't be updated.");
					}
				}

				if (tableInfo.Indexs != null && tableInfo.Indexs.Length > 0)
				{
					for (int i = 0; i < tableInfo.Indexs.Length; ++i)
					{
						var items = new HashSet<string>(tableInfo.Indexs[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()));

						if (items.Count == 0)
						{
							throw new SpiderException("Index should contain more than a column");
						}
						foreach (var item in items)
						{
							var column = columns.FirstOrDefault(c => c.Name == item);
							if (column == null)
							{
								throw new SpiderException("Columns set as index are not a property of your entity");
							}
							if (column.DataType == DataType.String && (column.Length <= 0 || column.Length > 256))
							{
								throw new SpiderException("Column length of index should not large than 256");
							}
						}
						tableInfo.Indexs[i] = string.Join(",", items);
					}
				}
				if (tableInfo.Uniques != null && tableInfo.Uniques.Length > 0)
				{
					for (int i = 0; i < tableInfo.Uniques.Length; ++i)
					{
						var items = new HashSet<string>(tableInfo.Uniques[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()));

						if (items.Count == 0)
						{
							throw new SpiderException("Unique should contain more than a column");
						}
						foreach (var item in items)
						{
							if (item.ToLower() == tableInfo.PrimaryKey)
							{
								continue;
							}
							var column = columns.FirstOrDefault(c => c.Name == item);
							if (column == null)
							{
								throw new SpiderException("Columns set as unique are not a property of your entity");
							}
							if (column.DataType == DataType.String && (column.Length <= 0 || column.Length > 256))
							{
								throw new SpiderException("Column length of unique should not large than 256");
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
