using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Model
{
	public class EntityDefine : AbstractSelector
	{
		public Table Table { get; set; }

		public List<Column> Columns { get; set; } = new List<Column>();

		public int Take { get; set; }

		public List<TargetUrlsSelector> TargetUrlsSelectors { get; set; }

		public List<LinkToNext> LinkToNexts { get; set; } = new List<LinkToNext>();

		public DataHandler DataHandler { get; set; }

		public List<SharedValueSelector> SharedValues { get; internal set; } = new List<SharedValueSelector>();
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