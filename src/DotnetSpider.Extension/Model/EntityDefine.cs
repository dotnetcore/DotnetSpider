using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Extension.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotnetSpider.Extension.Model
{
	public interface IEntityDefine
	{
		string Name { get; }
		BaseSelector Selector { get; }
		bool Multi { get; }
		Type Type { get; }
		EntityTable TableInfo { get; }
		List<Column> Columns { get; }
		int Take { get; }
		List<TargetUrlsSelector> TargetUrlsSelectors { get; }
		List<SharedValueSelector> SharedValues { get; }
	}

	public class EntityDefine<T> : IEntityDefine
	{
		public string Name { get; }

		public BaseSelector Selector { get; set; }

		public bool Multi { get; set; }

		public Type Type { get; }

		public EntityTable TableInfo { get; set; }

		public List<Column> Columns { get; set; } = new List<Column>();

		public int Take { get; set; }

		public List<TargetUrlsSelector> TargetUrlsSelectors { get; set; }

		public DataHandler<T> DataHandler { get; set; }

		public List<SharedValueSelector> SharedValues { get; internal set; }

		public EntityDefine()
		{
			Type = typeof(T);

			var typeName = Type.GetTypeCrossPlatform().FullName;
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
				Selector = new BaseSelector { Expression = entitySelector.Expression, Type = entitySelector.Type };
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

	public class Column
	{
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
			Selector = new BaseSelector
			{
				Expression = propertyDefine.Expression,
				Type = propertyDefine.Type,
				Argument = propertyDefine.Argument
			};
			NotNull = propertyDefine.NotNull;
			IgnoreStore = propertyDefine.IgnoreStore;
			Length = propertyDefine.Length;

			foreach (var formatter in property.GetCustomAttributes<Formatter.Formatter>(true))
			{
				Formatters.Add(formatter);
			}
		}

		public PropertyDefine PropertyDefine { get; }

		public PropertyInfo Property { get; }

		public object DefaultValue { get; }

		public string Name => Property.Name;

		public BaseSelector Selector { get; set; }

		public bool NotNull { get; set; }

		public PropertyDefine.Options Option { get; set; }

		public int Length { get; set; }

		public Type DataType => Property.PropertyType;

		public bool IgnoreStore { get; set; }

		public List<Formatter.Formatter> Formatters { get; set; } = new List<Formatter.Formatter>();

		public override string ToString()
		{
			return $"{Name},{DataType.Name}";
		}
	}
}