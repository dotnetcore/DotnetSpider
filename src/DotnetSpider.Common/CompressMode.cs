using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Common
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum CompressMode
	{
		None = 0,
		Gzip = 1,
		Lz4 = 2
	}
}
