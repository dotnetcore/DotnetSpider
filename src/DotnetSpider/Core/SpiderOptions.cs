using System;
using DotnetSpider.Data.Storage;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 任务选项
	/// </summary>
	public class SpiderOptions : ISpiderOptions
	{
		private readonly IConfiguration _configuration;

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		public string StorageConnectionString => _configuration["StorageConnectionString"];

		/// <summary>
		/// 存储器类型: FullTypeName, AssemblyName
		/// </summary>
		public string Storage => _configuration["Storage"];

		/// <summary>
		/// 是否忽略数据库相关的大写小
		/// </summary>
		public bool StorageIgnoreCase => string.IsNullOrWhiteSpace(_configuration["IgnoreCase"]) ||
		                                 bool.Parse(_configuration["StorageIgnoreCase"]);

		/// <summary>
		/// 存储器失败重试次数限制
		/// </summary>
		public int StorageRetryTimes => string.IsNullOrWhiteSpace(_configuration["StorageRetryTimes"])
			? 600
			: int.Parse(_configuration["StorageRetryTimes"]);

		/// <summary>
		/// 是否使用事务操作。默认不使用。
		/// </summary>
		public bool StorageUseTransaction => !string.IsNullOrWhiteSpace(_configuration["StorageUseTransaction"]) &&
		                                     bool.Parse(_configuration["StorageUseTransaction"]);

		/// <summary>
		/// 存储器类型
		/// </summary>
		public StorageType StorageType => string.IsNullOrWhiteSpace(_configuration["StorageType"])
			? StorageType.InsertIgnoreDuplicate
			: (StorageType) Enum.Parse(typeof(StorageType), _configuration["StorageType"]);

		/// <summary>
		/// MySql 文件类型
		/// </summary>
		public string MySqlFileType => _configuration["MySqlFileType"];

		/// <summary>
		/// 邮件服务地址
		/// </summary>
		public string EmailHost => _configuration["EmailHost"];

		/// <summary>
		/// 邮件用户名
		/// </summary>
		public string EmailAccount => _configuration["EmailAccount"];

		/// <summary>
		/// 邮件密码
		/// </summary>
		public string EmailPassword => _configuration["EmailPassword"];

		/// <summary>
		/// 邮件显示名称
		/// </summary>
		public string EmailDisplayName => _configuration["EmailDisplayName"];

		/// <summary>
		/// 邮件服务端口
		/// </summary>
		public string EmailPort => _configuration["EmailPort"];

		/// <summary>
		/// Kafka 服务地址
		/// </summary>
		public string KafkaBootstrapServers => _configuration["KafkaBootstrapServers"];

		/// <summary>
		/// Kafka 消费组
		/// </summary>
		public string KafkaConsumerGroup => string.IsNullOrWhiteSpace(_configuration["KafkaConsumerGroup"])
			? "DotnetSpider"
			: _configuration["KafkaConsumerGroup"];

		public SpiderOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}
	}
}