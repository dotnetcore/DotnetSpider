using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Extension.Model
{
	public class TableInfo
	{
		public Schema Schema { get; }

		public HashSet<Column> Columns { get; }

		public List<Column> Primary { get; }

		public Dictionary<string, List<Column>> Indexes { get; }

		public Dictionary<string, List<Column>> Uniques { get; }

		public List<Column> Updates { get; }

		public TableInfo(Type type)
		{
			var typeName = type.FullName;
			var name = typeName;

			Schema = type.GetCustomAttributes(typeof(Schema), true).First() as Schema;

			if (Schema == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(Schema.TableName))
			{
				Schema.TableName = type.Name.ToLowerInvariant();
			}

			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			Columns = new HashSet<Column>();
			Primary = new List<Column>();
			Indexes = new Dictionary<string, List<Column>>();
			Uniques = new Dictionary<string, List<Column>>();
			Updates = new List<Column>();

			foreach (var property in properties)
			{
				var column = property.GetCustomAttributes(typeof(Column), true).FirstOrDefault() as Column;

				if (column == null)
				{
					continue;
				}

				if (string.IsNullOrWhiteSpace(column.Name))
				{
					column.Name = property.Name;
				}
				if (column.DataType == DataType.None)
				{
					column.DataType = ConvertDataType(property.PropertyType);
				}
				Columns.Add(column);

				var primary = property.GetCustomAttributes(typeof(Primary), true).FirstOrDefault() as Primary;
				if (primary != null)
				{
					Primary.Add(column);
				}
				var indexes = new HashSet<Index>(property.GetCustomAttributes(typeof(Index), true).Select(i => (Index)i));
				foreach (var index in indexes)
				{
					if (string.IsNullOrWhiteSpace(index.Name))
					{
						index.Name = property.Name.ToUpperInvariant();
					}
					if (!Indexes.ContainsKey(index.Name))
					{
						Indexes.Add(index.Name, new List<Column>());
					}
					Indexes[index.Name].Add(column);
				}
				var uniques = new HashSet<Unique>(property.GetCustomAttributes(typeof(Unique), true).Select(i => (Unique)i));
				foreach (var unique in uniques)
				{
					if (string.IsNullOrWhiteSpace(unique.Name))
					{
						unique.Name = property.Name.ToUpperInvariant();
					}
					if (!Uniques.ContainsKey(unique.Name))
					{
						Uniques.Add(unique.Name, new List<Column>());
					}
					Uniques[unique.Name].Add(column);
				}

				var update = property.GetCustomAttributes(typeof(Update), true).FirstOrDefault() as Update;
				if (update != null)
				{
					Updates.Add(column);
				}
			}

			if (Columns.Count != properties.Count())
			{
				throw new ArgumentException($"Column names should not be same.");
			}

			if (Columns.Count == 0)
			{
				throw new ArgumentException($"Table should contains at least one column.");
			}
		}

		internal bool IsAutoIncrementPrimary => Primary.Count == 1 && Columns.Count(f => f.DataType == DataType.Int || f.DataType == DataType.Long) == 1;

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
