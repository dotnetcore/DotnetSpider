using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotnetSpider.Extraction.Model
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum TableNamePostfix
	{
		None,

		/// <summary>
		/// 表名的后缀为星期一的时间
		/// </summary>
		Monday,

		/// <summary>
		/// 表名的后缀为今天的时间 {name}_20171212
		/// </summary>
		Today,

		/// <summary>
		/// 表名的后缀为当月的第一天 {name}_20171201
		/// </summary>
		FirstDayOfTheMonth,

		/// <summary>
		/// 表名的后缀为当月 {name}_201712
		/// </summary>
		Month,

		/// <summary>
		/// 表名的后缀为上个月 {name}_201711
		/// </summary>
		LastMonth
	}

}
