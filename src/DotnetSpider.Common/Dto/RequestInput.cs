using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Common.Dto
{
	/// <summary>
	/// 请求结果
	/// </summary>
	public class RequestInput
	{
		/// <summary>
		/// Request 唯一标识
		/// </summary>
		public string Identity { get; set; }

		/// <summary>
		/// 当前链接重试的次数
		/// </summary>
		public int CycleTriedTimes { get; set; }

		/// <summary>
		/// 请求结果
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// 最终请求地址
		/// </summary>
		public string TargetUrl { get; set; }

		/// <summary>
		/// 请求完成时间
		/// </summary>
		public DateTime ResponseTime { get; set; }

		/// <summary>
		/// 请求结果状态码
		/// </summary>
		public int StatusCode { get; set; }
	}
}
