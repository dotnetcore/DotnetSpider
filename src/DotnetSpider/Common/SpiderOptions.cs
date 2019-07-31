using System;
using DotnetSpider.DataFlow.Storage;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Common
{
	/// <summary>
	/// 任务选项
	/// </summary>
	public class SpiderOptions
	{
		private readonly IConfiguration _configuration;

		public SpiderOptions(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		public virtual string ConnectionString => _configuration["ConnectionString"];

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		public virtual  string StorageConnectionString => _configuration["StorageConnectionString"];

		/// <summary>
		/// 存储器类型: FullTypeName, AssemblyName
		/// </summary>
		public virtual  string Storage => _configuration["Storage"];

		/// <summary>
		/// 是否忽略数据库相关的大写小
		/// </summary>
		public virtual  bool StorageIgnoreCase => string.IsNullOrWhiteSpace(_configuration["IgnoreCase"]) ||
		                                 bool.Parse(_configuration["StorageIgnoreCase"]);

		/// <summary>
		/// 存储器失败重试次数限制
		/// </summary>
		public virtual  int StorageRetryTimes => string.IsNullOrWhiteSpace(_configuration["StorageRetryTimes"])
			? 600
			: int.Parse(_configuration["StorageRetryTimes"]);

		/// <summary>
		/// 是否使用事务操作。默认不使用。
		/// </summary>
		public virtual  bool StorageUseTransaction => !string.IsNullOrWhiteSpace(_configuration["StorageUseTransaction"]) &&
		                                     bool.Parse(_configuration["StorageUseTransaction"]);

		/// <summary>
		/// 存储器类型
		/// </summary>
		public virtual  StorageType StorageType => string.IsNullOrWhiteSpace(_configuration["StorageType"])
			? StorageType.InsertIgnoreDuplicate
			: (StorageType) Enum.Parse(typeof(StorageType), _configuration["StorageType"]);

		/// <summary>
		/// MySql 文件类型
		/// </summary>
		public virtual  string MySqlFileType => _configuration["MySqlFileType"];

		/// <summary>
		/// 邮件服务地址
		/// </summary>
		public virtual  string EmailHost => _configuration["EmailHost"];

		/// <summary>
		/// 邮件用户名
		/// </summary>
		public virtual  string EmailAccount => _configuration["EmailAccount"];

		/// <summary>
		/// 邮件密码
		/// </summary>
		public virtual  string EmailPassword => _configuration["EmailPassword"];

		/// <summary>
		/// 邮件显示名称
		/// </summary>
		public virtual  string EmailDisplayName => _configuration["EmailDisplayName"];

		/// <summary>
		/// 邮件服务端口
		/// </summary>
		public virtual  int EmailPort => int.Parse(_configuration["EmailPort"]);

		public virtual  string TopicResponseHandler => "ResponseHandler-";

		public virtual  string TopicDownloaderAgentRegisterCenter => "DownloaderAgentRegisterCenter";

		public virtual  string TopicStatisticsService => "StatisticsService";

		public virtual  string TopicDownloadQueue =>"DownloadQueue";

		public virtual  string TopicAdslDownloadQueue => "AdslDownloadQueue";

		/// <summary>
		/// 消息队列推送消息、文章话题、获取消息失败重试的次数
		/// 默认是 28800 次即 8 小时
		/// </summary>
		public virtual  int MessageQueueRetryTimes => string.IsNullOrWhiteSpace(_configuration["MessageQueueRetryTimes"])
			? 28800
			: int.Parse(_configuration["MessageQueueRetryTimes"]);

		/// <summary>
		/// 设置消息过期时间，每个消息发送应该带上时间，超时的消息不作处理
		/// 默认值 60 秒
		/// </summary>
		public virtual  int MessageExpiredTime => string.IsNullOrWhiteSpace(_configuration["MessageExpiredTime"])
			? 60
			: int.Parse(_configuration["MessageExpiredTime"]);
	}
}
