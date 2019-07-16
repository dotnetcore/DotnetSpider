using System;

namespace DotnetSpider.Portal.Models.SpiderContainer
{
	public class ListSpiderContainerViewModel
	{
		/// <summary>
		/// 
		/// </summary>
		public int SpiderId { get; set; }

		/// <summary>
		/// 容器标识
		/// </summary>
		public string ContainerId { get; set; }

		public string Batch { get; set; }
		
		public string Status { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreationTime { get; set; }

		public DateTime? Start { get; set; }

		public DateTime? Exit { get; set; }

		public long Total { get; set; }

		public long Success { get; set; }

		public long Failed { get; set; }
	}
}