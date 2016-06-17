using Java2Dotnet.Spider.Extension.Model.Attribute;
using Java2Dotnet.Spider.Extension.Model.Formatter;
using Java2Dotnet.Spider.Extension.ORM;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Java2Dotnet.Spider.Extension.Configuration
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
	}

	public class Entity : DataToken
	{
		public List<DataToken> Fields { get; set; } = new List<DataToken>();
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
	}
}