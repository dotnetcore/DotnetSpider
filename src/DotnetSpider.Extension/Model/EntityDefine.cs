using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotnetSpider.Extension.Model
{
	/// <summary>
	/// 爬虫实体类的定义
	/// </summary>
	public interface IEntityDefine
	{
		/// <summary>
		/// 爬虫实体类的名称, 用于解析器和数据管道之间匹配. 默认是爬虫实体类的全称
		/// </summary>
		string Name { get; }

		/// <summary>
		/// 爬虫实体类的选择器
		/// </summary>
		SelectorAttribute SelectorAttribute { get; }

		/// <summary>
		/// 实体结果是否多个
		/// </summary>
		bool Multi { get; }

		/// <summary>
		/// 爬虫实体类的类型
		/// </summary>
		Type Type { get; }

		/// <summary>
		/// 爬虫实体对应的数据库表信息
		/// </summary>
		EntityTable TableInfo { get; }

		/// <summary>
		/// 爬虫实体定义的数据库列信息
		/// </summary>
		List<Column> Columns { get; }

		/// <summary>
		/// 从最终解析到的结果中取前 Take 个实体
		/// </summary>
		int Take { get; }

		/// <summary>
		/// 设置 Take 的方向, 默认是从头部取
		/// </summary>
		bool TakeFromHead { get; set; }

		/// <summary>
		/// 目标链接的选择器
		/// </summary>
		List<TargetUrlsSelector> TargetUrlsSelectors { get; }

		/// <summary>
		/// 共享值的选择器
		/// </summary>
		List<SharedValueSelector> SharedValues { get; }
	}

	/// <summary>
	/// 爬虫实体类的定义
	/// </summary>
	/// <typeparam name="T">爬虫实体类的类型</typeparam>
	public class EntityDefine<T> : IEntityDefine
	{
		/// <summary>
		/// 爬虫实体类的名称, 用于解析器和数据管道之间匹配. 默认是爬虫实体类的全称
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// 爬虫实体类的选择器
		/// </summary>
		public SelectorAttribute SelectorAttribute { get; set; }

		/// <summary>
		/// 实体结果是否多个
		/// </summary>
		public bool Multi { get; set; }

		/// <summary>
		/// 爬虫实体类的类型
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// 爬虫实体对应的数据库表信息
		/// </summary>
		public EntityTable TableInfo { get; set; }

		/// <summary>
		/// 爬虫实体定义的数据库列信息
		/// </summary>
		public List<Column> Columns { get; set; } = new List<Column>();

		/// <summary>
		/// 从最终解析到的结果中取前 Take 个实体
		/// </summary>
		public int Take { get; set; }

		/// <summary>
		/// 设置 Take 的方向, 默认是从头部取
		/// </summary>
		public bool TakeFromHead { get; set; } = true;

		/// <summary>
		/// 目标链接的选择器
		/// </summary>
		public List<TargetUrlsSelector> TargetUrlsSelectors { get; set; }

		/// <summary>
		/// 对Processor的结构结果进一步加工操作
		/// </summary>
		public DataHandler<T> DataHandler { get; set; }

		/// <summary>
		/// 共享值的选择器
		/// </summary>
		public List<SharedValueSelector> SharedValues { get; internal set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		public EntityDefine()
		{
			Type = typeof(T);

			var typeName = Type.FullName;
			Name = typeName;

			TableInfo = Type.GetCustomAttribute<EntityTable>();

			if (TableInfo != null)
			{
				if (TableInfo.Indexs != null)
				{
					TableInfo.Indexs = new HashSet<string>(TableInfo.Indexs.Select(i => i.Replace(" ", ""))).ToArray();
				}
				if (TableInfo.Uniques != null)
				{
					TableInfo.Uniques = new HashSet<string>(TableInfo.Uniques.Select(i => i.Replace(" ", ""))).ToArray();
				}
			}
			EntitySelector entitySelector = Type.GetCustomAttribute<EntitySelector>();
			if (entitySelector != null)
			{
				Multi = true;
				Take = entitySelector.Take;
				TakeFromHead = entitySelector.TakeFromHead;
				SelectorAttribute = new SelectorAttribute { Expression = entitySelector.Expression, Type = entitySelector.Type };
			}
			else
			{
				Multi = false;
			}
			var targetUrlsSelectors = Type.GetCustomAttributes<TargetUrlsSelector>();
			TargetUrlsSelectors = targetUrlsSelectors.ToList();
			var sharedValueSelectorAttributes = Type.GetCustomAttributes<SharedValueSelector>();
			SharedValues = sharedValueSelectorAttributes.Select(e => new SharedValueSelector
			{
				Name = e.Name,
				Expression = e.Expression,
				Type = e.Type
			}).ToList();

			GenerateEntityColumns();

			ValidateEntityDefine();
		}

		private void GenerateEntityColumns()
		{
			var properties = Type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			foreach (var property in properties)
			{
				var propertyDefine = property.GetCustomAttribute<PropertyDefine>();
				if (propertyDefine == null)
				{
					continue;
				}

				var column = new Column(property, propertyDefine);
				Columns.Add(column);
			}
		}

		private void ValidateEntityDefine()
		{
			var columns = Columns.Where(c => !c.IgnoreStore).ToList();

			if (columns.Count == 0)
			{
				throw new SpiderException($"Columns is necessary for {Name}.");
			}
			if (TableInfo == null)
			{
				return;
			}

			if (TableInfo.UpdateColumns != null && TableInfo.UpdateColumns.Length > 0)
			{
				foreach (var column in TableInfo.UpdateColumns)
				{
					if (columns.All(c => c.Name != column))
					{
						throw new SpiderException("Columns set to update are not a property of your entity.");
					}
				}
				var updateColumns = new List<string>(TableInfo.UpdateColumns);
				foreach (var id in Env.IdColumns)
				{
					updateColumns.Remove(id);
				}

				TableInfo.UpdateColumns = updateColumns.ToArray();

				if (TableInfo.UpdateColumns.Length == 0)
				{
					throw new SpiderException("There is no column need update.");
				}
			}

			if (TableInfo.Indexs != null && TableInfo.Indexs.Length > 0)
			{
				for (int i = 0; i < TableInfo.Indexs.Length; ++i)
				{
					var items = new HashSet<string>(TableInfo.Indexs[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()));

					if (items.Count == 0)
					{
						throw new SpiderException("Index should contain more than a column.");
					}
					if (items.Count == 1 && Env.IdColumns.Contains(items.First()))
					{
						throw new SpiderException("Primary is no need to create another index.");
					}
					foreach (var item in items)
					{
						var column = columns.FirstOrDefault(c => c.Name == item);
						if (column == null)
						{
							throw new SpiderException("Columns set as index are not a property of your entity.");
						}
						if (column.DataType.FullName == DataTypeNames.String && (column.Length <= 0 || column.Length > 256))
						{
							throw new SpiderException("Column length of index should not large than 256.");
						}
					}
					TableInfo.Indexs[i] = string.Join(",", items);
				}
			}
			if (TableInfo.Uniques != null && TableInfo.Uniques.Length > 0)
			{
				for (int i = 0; i < TableInfo.Uniques.Length; ++i)
				{
					var items = new HashSet<string>(TableInfo.Uniques[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()));

					if (items.Count == 0)
					{
						throw new SpiderException("Unique should contain more than a column.");
					}
					if (items.Count == 1 && Env.IdColumns.Contains(items.First()))
					{
						throw new SpiderException("Primary is no need to create another unique.");
					}
					foreach (var item in items)
					{
						var column = columns.FirstOrDefault(c => c.Name == item);
						if (column == null)
						{
							throw new SpiderException("Columns set as unique are not a property of your entity.");
						}
						if (column.DataType.FullName == DataTypeNames.String && (column.Length <= 0 || column.Length > 256))
						{
							throw new SpiderException("Column length of unique should not large than 256.");
						}
					}
					TableInfo.Uniques[i] = string.Join(",", items);
				}
			}
		}
	}

	/// <summary>
	/// 数据库列的定义
	/// </summary>
	public class Column
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="property">属性的信息</param>
		/// <param name="propertyDefine">属性的定义</param>
		public Column(PropertyInfo property, PropertyDefine propertyDefine)
		{
			Property = property;
			PropertyDefine = propertyDefine;

			if (DataType.FullName != DataTypeNames.String && propertyDefine.Length > 0)
			{
				throw new SpiderException("Only string property can set length.");
			}
			DefaultValue = Property.PropertyType.IsValueType ? Activator.CreateInstance(Property.PropertyType) : null;
			Option = propertyDefine.Option;
			SelectorAttribute = new SelectorAttribute
			{
				Expression = propertyDefine.Expression,
				Type = propertyDefine.Type,
				Arguments = propertyDefine.Arguments
			};
			NotNull = propertyDefine.NotNull;
			IgnoreStore = propertyDefine.IgnoreStore;
			Length = propertyDefine.Length;

			foreach (var formatter in property.GetCustomAttributes<Formatter.Formatter>(true))
			{
				Formatters.Add(formatter);
			}
		}

		/// <summary>
		/// 属性的定义
		/// </summary>
		public PropertyDefine PropertyDefine { get; }

		/// <summary>
		/// 属性的信息
		/// </summary>
		public PropertyInfo Property { get; }

		/// <summary>
		/// 属性的默认值
		/// </summary>
		public object DefaultValue { get; }

		/// <summary>
		/// 列的名称
		/// </summary>
		public string Name => Property.Name;

		/// <summary>
		/// 属性值的选择器
		/// </summary>
		public SelectorAttribute SelectorAttribute { get; set; }

		/// <summary>
		/// 属性值是否为空
		/// </summary>
		public bool NotNull { get; set; }

		/// <summary>
		/// 额外选项
		/// </summary>
		public PropertyDefine.Options Option { get; set; }

		/// <summary>
		/// 列的长度
		/// </summary>
		public int Length { get; set; }

		/// <summary>
		/// 属性的类型
		/// </summary>
		public Type DataType => Property.PropertyType;

		/// <summary>
		/// 是否不把此列数据保存到数据库
		/// </summary>
		public bool IgnoreStore { get; set; }

		/// <summary>
		/// 属性值的格式化
		/// </summary>
		public List<Formatter.Formatter> Formatters { get; set; } = new List<Formatter.Formatter>();

		/// <summary>
		/// 重载 ToString
		/// </summary>
		/// <returns>String</returns>
		public override string ToString()
		{
			return $"{Name},{DataType.Name}";
		}
	}
}