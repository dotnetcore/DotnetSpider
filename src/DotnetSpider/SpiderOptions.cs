using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Infrastructure;

namespace DotnetSpider
{
	public class SpiderOptions
	{
		/// <summary>
		/// 请求队列数限制
		/// </summary>
		public int RequestedQueueCount { get; set; } = 500;

		/// <summary>
		/// 请求链接深度限制
		/// </summary>
		public int Depth { get; set; }

		/// <summary>
		/// 请求 Timeout
		/// </summary>
		public int RequestTimeout { get; set; } = 30;

		/// <summary>
		/// 请求重试次数限制
		/// </summary>
		public int RetriedTimes { get; set; } = 3;

		/// <summary>
		/// 当队列中无链接超时后退出爬虫
		/// </summary>
		public int EmptySleepTime { get; set; } = 30;

		/// <summary>
		/// 爬虫采集速度，1 表示 1 秒钟一个请求，0.5 表示 1 秒钟 0.5 个请求，5 表示 1 秒钟发送 5 个请求
		/// </summary>
		public double Speed { get; set; } = 1;

		/// <summary>
		/// 测试代理是否正常的链接
		/// </summary>
		public string ProxyTestUrl { get; set; } = "http://www.baidu.com";

		/// <summary>
		/// 代理供应 API
		/// 一般代理供应商都会提供 API 请求返回可用的代理列表
		/// </summary>
		public string ProxySupplierUrl { get; set; }

		/// <summary>
		/// 是否使用代理
		/// </summary>
		public bool UseProxy { get; set; }

		/// <summary>
		/// 去除外链
		/// </summary>
		public bool RemoveOutboundLinks { get; set; } = false;

		/// <summary>
		/// 存储所用的数据库连接字符串
		/// </summary>
		public string StorageConnectionString { get; set; }

		/// <summary>
		/// 存储器类型: FullTypeName, AssemblyName
		/// </summary>
		public string Storage { get; set; }

		/// <summary>
		/// 存储模式
		/// </summary>
		public StorageMode StorageMode { get; set; } = StorageMode.InsertIgnoreDuplicate;

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// 数据库名
		/// </summary>
		public string Database { get; set; } = "dotnetspider";

		/// <summary>
		/// MySqlFile 文件类型
		/// </summary>
		public MySqlFileType MySqlFileType { get; set; }

		/// <summary>
		/// SqlServer 版本
		/// </summary>
		public SqlServerVersion SqlServerVersion { get; set; }

		/// <summary>
		/// HBase 的 RestServer 地址
		/// </summary>
		public string HBaseRestServer { get; set; }
	}
}
