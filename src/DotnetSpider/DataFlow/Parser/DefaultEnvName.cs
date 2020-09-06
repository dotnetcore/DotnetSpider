using DotnetSpider.Http;

namespace DotnetSpider.DataFlow.Parser
{
	/// <summary>
	/// 默认环境名
	/// </summary>
	public class DefaultEnvName
	{
		/// <summary>
		/// 实体索引
		/// </summary>
		public const string Entity_Index = "ENTITY_INDEX";

		/// <summary>
		/// GUID
		/// </summary>
		public const string GUID = "GUID";

		/// <summary>
		/// 日期，格式:yyyy-MM-dd
		/// </summary>
		public const string Date = "DATE";

		/// <summary>
		/// 今天的日期，格式:yyyy-MM-dd
		/// </summary>
		public const string ToDay = "TODAY";

		/// <summary>
		/// 时间，格式:yyyy-MM-dd HH:mm:ss
		/// </summary>
		public const string DateTime = "DATETIME";

		/// <summary>
		/// 今天的时间，格式:yyyy-MM-dd HH:mm:ss
		/// </summary>
		public const string Now = "NOW";

		/// <summary>
		/// 当前月份第一天的日期，格式:yyyy-MM-dd
		/// </summary>
		public const string Month = "MONTH";

		/// <summary>
		/// 当前时间所在周星期一的日期，格式:yyyy-MM-dd
		/// </summary>
		public const string Monday = "Monday";

		/// <summary>
		/// 任务标示ID,取至 <see cref="DataContext.Request" /> 的 <see cref="Request.Owner" />
		/// </summary>
		public const string Spider_Id = "SPIDER_ID";

		/// <summary>
		/// 请求的哈西值,取至 <see cref="DataContext.Request" /> 的 <see cref="Request.Hash" />
		/// </summary>
		public const string Request_Hash = "REQUEST_HASH";
	}
}
