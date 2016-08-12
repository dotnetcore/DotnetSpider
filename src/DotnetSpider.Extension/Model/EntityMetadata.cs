using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.ORM;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DotnetSpider.Extension.Model
{
	public class EntityMetadata
	{
		public Selector Selector { get; set; }
		public bool Multi { get; set; }
		public string Name { get; set; }
		public Schema Schema { get; set; }
		public List<string[]> Indexes { get; set; }
		public List<string[]> Uniques { get; set; }
		public string AutoIncrement { get; set; }
		public string[] Primary { get; set; }
		public Entity Entity { get; set; } = new Entity();
		public Stopping Stopping { get; set; }
		public string[] Updates { get; internal set; }
		public int? Limit { get; set; }
        /// <summary>
        /// The corresponding url pattern(s).
        /// </summary>
        public string[] TargetUrls { get; set; }

    }

    public class Entity : DataToken
	{
		public List<DataToken> Fields { get; set; } = new List<DataToken>();
        /// <summary>
        /// Key: The Url extractors generated from the entity definition. Value: the properties' name from which the extras of the new reuqest with get from.
        /// </summary>
        public Dictionary<TargetUrlExtractor, List<string>> UrlExtras { get; set; }
            = new Dictionary<TargetUrlExtractor, List<string>>();

    }

    public class Field : DataToken
	{
		public string DataType { get; set; }
		public PropertyExtractBy.ValueOption Option { get; set; }
		public List<JObject> Formatters { get; set; } = new List<JObject>();
	}

	public abstract class DataToken
	{
		public Selector Selector { get; set; }
		public bool Multi { get; set; }
		public string Name { get; set; }
        public string Pattern { get; set; }
        public string ReplaceString { get; set; }
    }
}