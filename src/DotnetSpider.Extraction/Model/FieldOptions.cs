using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
		/// For html content
		/// </summary>
		OuterHtml,

		/// <summary>
		/// For html content
		/// </summary>
		InnerHtml,

		/// <summary>
		/// For html content
		/// </summary>
		InnerText,

		/// <summary>
		/// 取的查询器结果的个数作为结果
		/// </summary>
		Count
	}
}
