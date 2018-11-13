using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotnetSpider.Extension.Model
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DataType
	{
		None,
		Int,
		Float,
		Double,
		DateTime,
		Date,
		Long,
		Bool,
		String,
		Decimal
	}
}
