using System;
using System.ComponentModel.DataAnnotations;

namespace DotnetSpider.Common
{
	/// <summary>
	/// 请求记录, 需要对任务标识、请求标识做唯一索引
	/// </summary>
	public class RequestQueue
	{
		/// <summary>
		/// Request 唯一标识
		/// </summary>
		[Key]
		[StringLength(32)]
		public string RequestId { get; set; }

		/// <summary>
		/// 任务实例唯一标识
		/// </summary>
		[Key]
		[StringLength(32)]
		public string Identity { get; set; }

		/// <summary>
		/// 请求所属块
		/// </summary>
		[Required]
		[StringLength(32)]
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
		public int StatusCode { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// 完成时间
		/// </summary>
		public DateTime? ResponseTime { get; set; }

		/// <summary>
		/// 解析时间
		/// </summary>
		public DateTime? ProcessTime { get; set; }
	}
}
