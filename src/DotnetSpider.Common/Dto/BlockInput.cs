using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DotnetSpider.Common.Dto
{
	/// <summary>
	/// 任务块执行结果
	/// </summary>
	public class BlockInput
	{
		/// <summary>
		/// 任务块标识
		/// </summary>
		public string BlockId { get; set; }

		/// <summary>
		/// 实例标识
		/// </summary>
		public string Identity { get; set; }

		/// <summary>
		/// 请求结果
		/// </summary>
		public List<RequestInput> Results { get; set; }

		/// <summary>
		/// 如果执行异常则上报异常信息
		/// </summary>
		public string Exception { get; set; }
	}
}
