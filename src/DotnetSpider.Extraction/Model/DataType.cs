using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotnetSpider.Extraction.Model
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
