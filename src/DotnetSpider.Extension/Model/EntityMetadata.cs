using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Model
{
	public class EntityMetadata
	{
		public Table Table { get; set; }
		public Entity Entity { get; set; } = new Entity();
		public int? Limit { get; set; }
		public List<TargetUrlsSelector> TargetUrlsSelectors { get; set; }
		public DataHandler DataHandler { get; set; }
		public List<SharedValueSelector> SharedValues { get; internal set; } = new List<SharedValueSelector>();
	}

	public class Entity : DataToken
	{
		public List<DataToken> Fields { get; set; } = new List<DataToken>();
		public List<LinkToNext> TargetUrls { get; set; } = new List<LinkToNext>();
	}

	public class Field : DataToken
	{
		public PropertyDefine.Options Option { get; set; }
		public int Length { get; set; }
		public bool Store { get; set; }
		public List<Formatter.Formatter> Formatters { get; set; } = new List<Formatter.Formatter>();
	}

	public abstract class DataToken
	{
		public BaseSelector Selector { get; set; }
		public bool NotNull { get; set; }
		public bool Multi { get; set; }
		public string Name { get; set; }
	}
}