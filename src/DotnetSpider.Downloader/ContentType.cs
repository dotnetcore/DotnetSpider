using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotnetSpider.Downloader
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ContentType
	{
		Auto,
		Html,
		Json
	}
}