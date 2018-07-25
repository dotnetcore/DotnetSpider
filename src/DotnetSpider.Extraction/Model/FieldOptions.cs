using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotnetSpider.Extraction.Model
{
	/// <summary>
	/// 额外选项的定义
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum FieldOptions
	{
		/// <summary>
		/// 不作任何操作
		/// </summary>
		None,

		/// <summary>
		/// For html contene
		/// </summary>
		OuterHtml,

		/// <summary>
		/// For html contene
		/// </summary>
		InnerHtml,

		/// <summary>
		/// For html contene
		/// </summary>
		InnerText,

		/// <summary>
		/// 取的查询器结果的个数作为结果
		/// </summary>
		Count
	}
}
