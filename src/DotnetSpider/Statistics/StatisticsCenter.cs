using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Common;
using DotnetSpider.EventBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Statistics
{
	/// <summary>
	/// 统计服务中心
	/// </summary>
	public class StatisticsCenter : BackgroundService, IStatisticsCenter, IRunnable
	{
		private readonly IEventBus _eventBus;
		private readonly ILogger _logger;
		private readonly IStatisticsStore _statisticsStore;
		private readonly SpiderOptions _options;

		public bool IsRunning { get; private set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="eventBus">消息队列接口</param>
		/// <param name="options"></param>
		/// <param name="statisticsStore">统计存储接口</param>
		/// <param name="logger">日志接口</param>
		public StatisticsCenter(IEventBus eventBus, SpiderOptions options, IStatisticsStore statisticsStore,
			ILogger<StatisticsCenter> logger)
		{
			_options = options;
			_eventBus = eventBus;
			_statisticsStore = statisticsStore;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await _statisticsStore.EnsureDatabaseAndTableCreatedAsync();
			_logger.LogInformation("统计中心准备数据库完成");
			_eventBus.Subscribe(_options.StatisticsServiceTopic,
				async message => await HandleStatisticsMessageAsync(message));
			_logger.LogInformation("统计中心启动");
			IsRunning = true;
		}

		/// <summary>
		/// 停止统计中心
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_eventBus.Unsubscribe(_options.StatisticsServiceTopic);
			_logger.LogInformation("统计中心退出");
			return base.StopAsync(cancellationToken);
		}

		private async Task HandleStatisticsMessageAsync(Event message)
		{
			if (string.IsNullOrWhiteSpace(message.Data))
			{
				_logger.LogWarning($"接收到空消息");
				return;
			}

			switch (message.Type)
			{
				case "Success":
				{
					var ownerId = message.Data;
					await _statisticsStore.IncrementSuccessAsync(ownerId);
					break;
				}

				case "Failed":
				{
					var data = message.Data.Split(',');
					await _statisticsStore.IncrementFailedAsync(data[0], int.Parse(data[1]));
					break;
				}

				case "Start":
				{
					var ownerId = message.Data;
					await _statisticsStore.StartAsync(ownerId);
					break;
				}

				case "Exit":
				{
					var ownerId = message.Data;
					await _statisticsStore.ExitAsync(ownerId);
					break;
				}

				case "Total":
				{
					var data = message.Data.Split(',');
					await _statisticsStore.IncrementTotalAsync(data[0], int.Parse(data[1]));

					break;
				}

				case "DownloadSuccess":
				{
					var data = message.Data.Split(',');
					await _statisticsStore.IncrementDownloadSuccessAsync(data[0], int.Parse(data[1]),
						long.Parse(data[2]));
					break;
				}

				case "DownloadFailed":
				{
					var data = message.Data.Split(',');
					await _statisticsStore.IncrementDownloadFailedAsync(data[0], int.Parse(data[1]),
						long.Parse(data[2]));
					break;
				}

				case "Print":
				{
					var ownerId = message.Data;
					var statistics = await _statisticsStore.GetSpiderStatisticsAsync(ownerId);
					if (statistics != null)
					{
						_logger.LogInformation(
							$"任务 {ownerId} 总计 {statistics.Total}, 成功 {statistics.Success}, 失败 {statistics.Failed}, 剩余 {statistics.Total - statistics.Success - statistics.Failed}");
					}

					break;
				}
			}
		}
	}
}