using DotnetSpider.Downloader;
using System.Collections.Generic;

namespace DotnetSpider.Broker.Dtos
{
	public class BlockDto
	{
		/// <summary>
		/// 任务块标识
		/// </summary>
		public string BlockId { get; set; }

		/// <summary>
		/// 爬虫对象的标识
		/// </summary>
		public string Identity { get; set; }

		/// <summary>
		/// 需要下载的请求
		/// </summary>
		public List<Request> Requests { get; set; }

		/// <summary>
		/// 下载的启用线程数
		/// </summary>
		public int ThreadNum { get; set; }
	}
}
