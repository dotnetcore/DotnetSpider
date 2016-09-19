using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Model
{
	public class EntityMetadata
	{
		public Schema Schema { get; set; }
		public List<string[]> Indexes { get; set; }
		public List<string[]> Uniques { get; set; }
		public List<string> AutoIncrement { get; set; }
		public List<string> Primary { get; set; }
		public Entity Entity { get; set; } = new Entity();
		public List<string> Updates { get; internal set; }
		public int? Limit { get; set; }
		public List<TargetUrlExtractor> TargetUrlExtractors = new List<TargetUrlExtractor>();
		//public List<TargetUrlsCreator> TargetUrlsCreators { get; set; }
		public DataHandler DataHandler { get; set; }
	}

	public class Entity : DataToken
	{
		public List<DataToken> Fields { get; set; } = new List<DataToken>();
		public List<TargetUrl> TargetUrls { get; set; } = new List<TargetUrl>();
	}

	public class Field : DataToken
	{
		public string DataType { get; set; }
		public PropertySelector.ValueOption Option { get; set; }
		public List<Formatter.Formatter> Formatters { get; set; } = new List<Formatter.Formatter>();
	}

	public abstract class DataToken
	{
		public BaseSelector Selector { get; set; }
		public bool Multi { get; set; }
		public string Name { get; set; }
	}
}