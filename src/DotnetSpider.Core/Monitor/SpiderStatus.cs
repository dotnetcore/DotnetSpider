namespace DotnetSpider.Core.Monitor
{
	/// <summary>
	/// 爬虫状态
	/// </summary>
	public struct SpiderStatus
	{
		/// <summary>
		/// 任务编号
		/// </summary>
		public string TaskId;

		/// <summary>
		/// 唯一标识
		/// </summary>
		public string Identity;

		/// <summary>
		/// 爬虫节点编号
		/// </summary>
		public string NodeId;

		/// <summary>
		/// 爬虫状态: 运行、暂停、退出、完成
		/// </summary>
		public string Status;

		/// <summary>
		/// 爬虫线程数
		/// </summary>
		public int Thread;

		/// <summary>
		/// 剩余的目标链接数
		/// </summary>
		public long Left;

		/// <summary>
		/// 成功采集的链接数
		/// </summary>
		public long Success;

		/// <summary>
		/// 采集出错的链接数
		/// </summary>
		public long Error;

		/// <summary>
		/// 总的需要采集的链接数
		/// </summary>
		public long Total;

		/// <summary>
		/// 平均下载一个链接需要的时间(豪秒)
		/// </summary>
		public float AvgDownloadSpeed;

		/// <summary>
		/// 平均解析一个页面需要的时间(豪秒)
		/// </summary>
		public float AvgProcessorSpeed;

		/// <summary>
		/// 数据管道处理一次数据结果需要的时间(豪秒)
		/// </summary>
		public float AvgPipelineSpeed;
	}
}