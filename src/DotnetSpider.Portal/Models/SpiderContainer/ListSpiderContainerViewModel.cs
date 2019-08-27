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
		public DateTimeOffset CreationTime { get; set; }

		public DateTimeOffset? Start { get; set; }

		public DateTimeOffset? Exit { get; set; }

		public long Total { get; set; }

		public long Success { get; set; }

		public long Failed { get; set; }
	}
}
