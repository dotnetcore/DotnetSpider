using DotnetSpider.Data.Storage;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 任务选项
	/// </summary>
	public interface ISpiderOptions
	{
		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		string ConnectionString { get; }

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		string StorageConnectionString { get; }

		/// <summary>
		/// 存储器类型: FullTypeName, AssemblyName
		/// </summary>
		string Storage { get; }

		/// <summary>
		/// 存储器类型
		/// </summary>
		StorageType StorageType { get; }

		/// <summary>
		/// MySql 文件类型
		/// </summary>
		string MySqlFileType { get; }

		/// <summary>
		/// 是否忽略数据库相关的大写小
		/// </summary>
		bool StorageIgnoreCase { get; }

		/// <summary>
		/// 存储器失败重试次数限制
		/// </summary>
		int StorageRetryTimes { get; }

		/// <summary>
		/// 是否使用事务操作。默认不使用。
		/// </summary>
		bool StorageUseTransaction { get; }

		/// <summary>
		/// 邮件服务地址
		/// </summary>
		string EmailHost { get; }

		/// <summary>
		/// 邮件用户名
		/// </summary>
		string EmailAccount { get; }

		/// <summary>
		/// 邮件密码
		/// </summary>
		string EmailPassword { get; }

		/// <summary>
		/// 邮件显示名称
		/// </summary>
		string EmailDisplayName { get; }

		/// <summary>
		/// 邮件服务端口
		/// </summary>
		string EmailPort { get; }

		/// <summary>
		/// Kafka 服务地址
		/// </summary>
		string KafkaBootstrapServers { get; }

		/// <summary>
		/// Kafka 消费组
		/// </summary>
		string KafkaConsumerGroup { get; }
	}
}