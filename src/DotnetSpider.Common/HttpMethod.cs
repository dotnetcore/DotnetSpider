using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotnetSpider.Common
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum HttpMethod
	{
		Get,
		Post,
		Put,
		Delete,
		Head,
		Options,
		Patch,
		Trace
	}
}