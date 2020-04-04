using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Infrastructure;

namespace DotnetSpider
{
	public class SpiderOptions
	{
		public int RequestedQueueCount { get; set; } = 100;
		public int Depth { get; set; }
		public int RequestTimeout { get; set; } = 10;
		public int RetriedTimes { get; set; } = 3;
		public int EmptySleepTime { get; set; } = 30;
		public double Speed { get; set; } = 1;
		public string ProxyTestUri { get; set; } = "http://www.baidu.com";
		public string ProxySupplierUri { get; set; }
		public bool UseProxy { get; set; }

		public bool RemoveOutboundLinks { get; set; } = false;

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		public string StorageConnectionString { get; set; }

		/// <summary>
		/// 存储器类型: FullTypeName, AssemblyName
		/// </summary>
		public string Storage { get; set; }

		public string ConnectionString { get; set; }

		public string Database { get; set; } = "dotnetspider";

		public StorageMode StorageMode { get; set; } = StorageMode.InsertIgnoreDuplicate;

		public MySqlFileType MySqlFileType { get; set; }

		public SqlServerVersion SqlServerVersion { get; set; }
		public string HBaseRestServer { get; set; }
	}
}
