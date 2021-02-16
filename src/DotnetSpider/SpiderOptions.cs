namespace DotnetSpider
{
	public class SpiderOptions
	{
		/// <summary>
		/// 请求队列数限制
		/// </summary>
		public int RequestedQueueCount { get; set; } = 1000;

		/// <summary>
		/// 请求链接深度限制
		/// </summary>
		public int Depth { get; set; }

		/// <summary>
		/// 请求重试次数限制
		/// </summary>
		public int RetriedTimes { get; set; } = 3;

		/// <summary>
		/// 当队列中无链接超时后退出爬虫
		/// </summary>
		public int EmptySleepTime { get; set; } = 60;

		/// <summary>
		/// 爬虫采集速度，1 表示 1 秒钟一个请求，0.5 表示 1 秒钟 0.5 个请求，5 表示 1 秒钟 5 个请求
		/// </summary>
		public double Speed { get; set; } = 1;

		/// <summary>
		/// 一次请求队列获取多少个请求
		/// </summary>
		public uint Batch { get; set; } = 4;

		/// <summary>
		/// 是否去除外链
		/// </summary>
		public bool RemoveOutboundLinks { get; set; } = false;

		/// <summary>
		/// 存储器类型: FullTypeName, AssemblyName
		/// </summary>
		public string StorageType { get; set; } = "DotnetSpider.MySql.MySqlEntityStorage, DotnetSpider.MySql";

		/// <summary>
		/// 获取新代码的时间间隔
		/// </summary>
		public int RefreshProxy { get; set; } = 30;
	}
}
