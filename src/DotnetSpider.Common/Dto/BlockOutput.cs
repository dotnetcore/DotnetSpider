using DotnetSpider.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotnetSpider.Common.Dto
{
	/// <summary>
	/// 任务块命令
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Command
	{
		None = 0,
		Download = 1,
		Pause = 2,
		Continue = 3,
		Exit = 4
	}

	/// <summary>
	/// 任务块
	/// </summary>
	public class BlockOutput
	{
		/// <summary>
		/// 任务块标识
		/// </summary>
		public string BlockId { get; set; }

		/// <summary>
		/// 需要运行的命令
		/// </summary>
		public Command Command { get; set; }

		/// <summary>
		/// 爬虫对象的标识
		/// </summary>
		public string Identity { get; set; }

		/// <summary>
		/// 需要下载的请求
		/// </summary>
		public List<RequestOutput> Requests { get; set; }

		/// <summary>
		/// 站点信息
		/// </summary>
		public Site Site { get; set; }

		/// <summary>
		/// 下载的启用线程数
		/// </summary>
		public int ThreadNum { get; set; }
	}
}
