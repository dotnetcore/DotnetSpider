using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotnetSpider.Common
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ContentType
	{
		Auto,
		Html,
		Json
	}
}