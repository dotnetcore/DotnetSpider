using DotnetSpider.DataFlow.Storage;

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

		/// <summary>
		/// 消息队列推送消息、文章话题、获取消息失败重试的次数
		/// 默认为 2880 秒, 8 小时
		/// </summary>
		int MessageQueueRetryTimes { get; }

		/// <summary>
		/// 设置消息过期时间，每个消息发送应该带上时间，超时的消息不作处理
		/// 默认值 60 秒
		/// 过期时间必须小于下载中心同步数据的心跳时间
		/// </summary>
		int MessageExpiredTime { get; }
	}
}