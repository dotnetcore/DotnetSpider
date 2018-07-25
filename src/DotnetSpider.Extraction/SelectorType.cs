using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace DotnetSpider.Extraction
{
	/// <summary>
	/// 查询器类型
	/// </summary>
	[Flags]
	[JsonConverter(typeof(StringEnumConverter))]
	public enum SelectorType
	{
		/// <summary>
		/// XPath
		/// </summary>
		XPath,
		/// <summary>
		/// Regex
		/// </summary>
		Regex,
		/// <summary>
		/// Css
		/// </summary>
		Css,
		/// <summary>
		/// JsonPath
		/// </summary>
		JsonPath,
		/// <summary>
		/// Enviroment
		/// </summary>
		Enviroment
	}
}
