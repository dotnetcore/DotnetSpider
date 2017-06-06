using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Model
{
	public class Entity : AbstractSelector
	{
		public Table Table { get; set; }
		public int Take { get; set; }
		public List<TargetUrlsSelector> TargetUrlsSelectors { get; set; }
		public List<Field> Fields { get; set; } = new List<Field>();
		public List<LinkToNext> LinkToNexts { get; set; } = new List<LinkToNext>();
		public DataHandler DataHandler { get; set; }
		public List<SharedValueSelector> SharedValues { get; internal set; } = new List<SharedValueSelector>();
	}

	public class Field : AbstractSelector
	{
		public PropertyDefine.Options Option { get; set; }
		public int Length { get; set; }
		public DataType DataType { get; set; }
		public bool IgnoreStore { get; set; }
		public List<Formatter.Formatter> Formatters { get; set; } = new List<Formatter.Formatter>();
	}

	public enum DataType
	{
		Int,
		Bigint,
		Text,
		Float,
		Double,
		Time
	}

	public abstract class AbstractSelector
	{
		public BaseSelector Selector { get; set; }
		public bool NotNull { get; set; }
		public bool Multi { get; set; }
		public string Name { get; set; }
	}
}