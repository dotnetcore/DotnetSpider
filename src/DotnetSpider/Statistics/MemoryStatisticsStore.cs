using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Statistics.Entity;

namespace DotnetSpider.Statistics
{
	/// <summary>
	/// 基于内存的统计存储
	/// </summary>
	public class MemoryStatisticsStore : IStatisticsStore
	{
		private readonly ConcurrentDictionary<string, SpiderStatistics> _spiderStatisticsDict =
			new ConcurrentDictionary<string, SpiderStatistics>();

		private readonly ConcurrentDictionary<string, DownloadStatistics> _downloadStatisticsDict =
			new ConcurrentDictionary<string, DownloadStatistics>();

		/// <summary>
		/// 添加总请求数
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <param name="count">请求数</param>
		/// <returns></returns>
		public Task IncrementTotalAsync(string ownerId, int count)
		{
			_spiderStatisticsDict.AddOrUpdate(ownerId, s => new SpiderStatistics
			{
				Total = count
			}, (s, statistics) =>
			{
				statistics.AddTotal(count);
				return statistics;
			});
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		public Task EnsureDatabaseAndTableCreatedAsync()
		{
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		/// <summary>
		/// 增加成功次数 1
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <returns></returns>
		public Task IncrementSuccessAsync(string ownerId)
		{
			_spiderStatisticsDict.AddOrUpdate(ownerId, s => new SpiderStatistics
			{
				Success = 1
			}, (s, statistics) =>
			{
				statistics.IncSuccess();
				return statistics;
			});
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		/// <summary>
		/// 添加指定失败次数
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <param name="count">失败次数</param>
		/// <returns></returns>
		public Task IncrementFailedAsync(string ownerId, int count = 1)
		{
			_spiderStatisticsDict.AddOrUpdate(ownerId, s => new SpiderStatistics
			{
				Failed = count
			}, (s, statistics) =>
			{
				statistics.AddFailed(count);
				return statistics;
			});
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		/// <summary>
		/// 设置爬虫启动时间
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <returns></returns>
		public Task StartAsync(string ownerId)
		{
			_spiderStatisticsDict.AddOrUpdate(ownerId, s => new SpiderStatistics
			{
				Start = DateTime.Now
			}, (s, statistics) =>
			{
				statistics.Start = DateTime.Now;
				return statistics;
			});
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		/// <summary>
		/// 设置爬虫退出时间
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <returns></returns>
		public Task ExitAsync(string ownerId)
		{
			_spiderStatisticsDict.AddOrUpdate(ownerId, s => new SpiderStatistics
			{
				Exit = DateTime.Now
			}, (s, statistics) =>
			{
				statistics.Exit = DateTime.Now;
				return statistics;
			});
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		/// <summary>
		/// 添加指定下载代理器的下载成功次数
		/// </summary>
		/// <param name="agentId">下载代理器标识</param>
		/// <param name="count">下载成功次数</param>
		/// <param name="elapsedMilliseconds">下载总消耗的时间</param>
		/// <returns></returns>
		public Task IncrementDownloadSuccessAsync(string agentId, int count, long elapsedMilliseconds)
		{
			_downloadStatisticsDict.AddOrUpdate(agentId, s => new DownloadStatistics
			{
				AgentId = agentId,
				Success = count,
				ElapsedMilliseconds = elapsedMilliseconds
			}, (s, statistics) =>
			{
				statistics.AddSuccess(count);
				statistics.AddElapsedMilliseconds(elapsedMilliseconds);
				return statistics;
			});
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		/// <summary>
		/// 添加指定下载代理器的下载失败次数
		/// </summary>
		/// <param name="agentId">下载代理器标识</param>
		/// <param name="count">下载失败次数</param>
		/// <param name="elapsedMilliseconds">下载总消耗的时间</param>
		/// <returns></returns>
		public Task IncrementDownloadFailedAsync(string agentId, int count, long elapsedMilliseconds)
		{
			_downloadStatisticsDict.AddOrUpdate(agentId, s => new DownloadStatistics
			{
				AgentId = agentId,
				Failed = count,
				ElapsedMilliseconds = elapsedMilliseconds
			}, (s, statistics) =>
			{
				statistics.AddFailed(count);
				statistics.AddElapsedMilliseconds(elapsedMilliseconds);
				return statistics;
			});
#if NETFRAMEWORK
            return DotnetSpider.Core.Framework.CompletedTask;
#else
			return Task.CompletedTask;
#endif
		}

		/// <summary>
		/// 分页查询下载代理器的统计信息
		/// </summary>
		/// <param name="page"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public Task<List<DownloadStatistics>> GetDownloadStatisticsListAsync(int page, int size)
		{
			return Task.FromResult(_downloadStatisticsDict.Values.ToList());
		}

		/// <summary>
		/// 查询指定下载代理器的统计信息
		/// </summary>
		/// <param name="agentId">下载代理器标识</param>
		/// <returns></returns>
		public Task<DownloadStatistics> GetDownloadStatisticsAsync(string agentId)
		{
			return Task.FromResult(_downloadStatisticsDict.TryGetValue(agentId, out var statistics)
				? statistics
				: null);
		}

		/// <summary>
		/// 查询指定爬虫的统计信息
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <returns></returns>
		public Task<SpiderStatistics> GetSpiderStatisticsAsync(string ownerId)
		{
			return Task.FromResult(_spiderStatisticsDict.TryGetValue(ownerId, out var statistics) ? statistics : null);
		}

		/// <summary>
		/// 分页查询爬虫的统计信息
		/// </summary>
		/// <param name="page"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public Task<List<SpiderStatistics>> GetSpiderStatisticsListAsync(int page, int size)
		{
			return Task.FromResult(_spiderStatisticsDict.Values.ToList());
		}
	}
}