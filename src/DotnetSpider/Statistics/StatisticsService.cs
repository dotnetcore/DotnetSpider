using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.MessageQueue;

namespace DotnetSpider.Statistics
{
	/// <summary>
	/// 统计服务
	/// </summary>
	public class StatisticsService : IStatisticsService
	{
		private readonly IMq _mq;
		private readonly SpiderOptions _options;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="eventBus">消息队列接口</param>
		/// <param name="options"></param>
		public StatisticsService(IMq eventBus, SpiderOptions options)
		{
			_mq = eventBus;
			_options = options;
		}

		/// <summary>
		/// 增加成功次数 1
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <returns></returns>
		public async Task IncrementSuccessAsync(string ownerId)
		{
			await _mq.PublishAsync(_options.TopicStatisticsService,
				new MessageData<string> {Type = "Success", Data = ownerId});
		}

		/// <summary>
		/// 添加指定失败次数
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <param name="count">失败次数</param>
		/// <returns></returns>
		public async Task IncrementFailedAsync(string ownerId, int count = 1)
		{
			await _mq.PublishAsync(_options.TopicStatisticsService,
				new MessageData<string> {Type = "Failed", Data = $"{ownerId},{count}"});
		}

		/// <summary>
		/// 设置爬虫启动时间
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <returns></returns>
		public async Task StartAsync(string ownerId)
		{
			await _mq.PublishAsync(_options.TopicStatisticsService,
				new MessageData<string> {Type = "Start", Data = ownerId});
		}

		/// <summary>
		/// 设置爬虫退出时间
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <returns></returns>
		public async Task ExitAsync(string ownerId)
		{
			await _mq.PublishAsync(_options.TopicStatisticsService,
				new MessageData<string> {Type = "Exit", Data = ownerId});
		}

		/// <summary>
		/// 添加指定下载代理器的下载成功次数
		/// </summary>
		/// <param name="agentId">下载代理器标识</param>
		/// <param name="count">下载成功次数</param>
		/// <param name="elapsedMilliseconds">下载总消耗的时间</param>
		/// <returns></returns>
		public async Task IncrementDownloadSuccessAsync(string agentId, int count, long elapsedMilliseconds)
		{
			await _mq.PublishAsync(_options.TopicStatisticsService,
				new MessageData<string> {Type = "DownloadSuccess", Data = $"{agentId},{count},{elapsedMilliseconds}"});
		}

		/// <summary>
		/// 添加指定下载代理器的下载失败次数
		/// </summary>
		/// <param name="agentId">下载代理器标识</param>
		/// <param name="count">下载失败次数</param>
		/// <param name="elapsedMilliseconds">下载总消耗的时间</param>
		/// <returns></returns>
		public async Task IncrementDownloadFailedAsync(string agentId, int count, long elapsedMilliseconds)
		{
			await _mq.PublishAsync(_options.TopicStatisticsService,
				new MessageData<string> {Type = "DownloadFailed", Data = $"{agentId},{count},{elapsedMilliseconds}"});
		}

		/// <summary>
		/// 打印统计信息(仅限本地爬虫使用)
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <returns></returns>
		public async Task PrintStatisticsAsync(string ownerId)
		{
			await _mq.PublishAsync(_options.TopicStatisticsService,
				new MessageData<string> {Type = "Print", Data = ownerId});
		}

		/// <summary>
		/// 添加总请求数
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <param name="count">请求数</param>
		/// <returns></returns>
		public async Task IncrementTotalAsync(string ownerId, int count)
		{
			await _mq.PublishAsync(_options.TopicStatisticsService,
				new MessageData<string> {Type = "Total", Data = $"{ownerId},{count}"});
		}
	}
}
