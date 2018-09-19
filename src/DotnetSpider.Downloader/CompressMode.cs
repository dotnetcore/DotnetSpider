using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotnetSpider.Downloader
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum CompressMode
	{
		None = 0,
		Gzip = 1,
		Lz4 = 2
	}
}
