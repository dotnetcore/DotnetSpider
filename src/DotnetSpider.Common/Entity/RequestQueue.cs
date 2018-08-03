using System;

namespace DotnetSpider.Common
{
	/// <summary>
	/// 请求记录, 需要对任务标识、请求标识做唯一索引
	/// </summary>
	public class RequestQueue
	{
		/// <summary>
		/// 自增记录编号
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// Request 唯一标识
		/// </summary>
		public string RequestId { get; set; }

		/// <summary>
		/// 任务实例唯一标识
		/// </summary>
		public string Identity { get; set; }

		/// <summary>
		/// 请求所属块
		/// </summary>
		public string BlockId { get; set; }

		/// <summary>
		/// 序列化内容
		/// </summary>
		public string Request { get; set; }

		/// <summary>
		/// 请求结果的序列化
		/// </summary>
		public string Response { get; set; }

		/// <summary>	
		/// 请求结果编码
		/// </summary>
		public int HttpStatusCode { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// 完成时间
		/// </summary>
		public DateTime ResponseTime { get; set; }

		/// <summary>
		/// 解析时间
		/// </summary>
		public DateTime? ProcessTime { get; set; }
	}
}
