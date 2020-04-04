namespace DotnetSpider.Portal.ViewObject
{
	public class SpiderHistoryViewObject
	{
		/// <summary>
		/// 主键
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		///
		/// </summary>
		public string SpiderName { get; set; }

		/// <summary>
		/// 容器标识
		/// </summary>
		public string ContainerId { get; set; }

		/// <summary>
		/// 容器标识
		/// </summary>
		public string Batch { get; set; }


		public string Status { get; set; }

		public long Left { get; set; }
		public long Total { get; set; }
		public long Success { get; set; }
		public long Failure { get; set; }

		public string Start { get; set; }

		public string Exit { get; set; }
	}
}
