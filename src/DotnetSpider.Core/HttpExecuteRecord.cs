using DotnetSpider.Common;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Downloader;
using Polly;
using System;
using System.Threading;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 通过Http向企业服务上报运行状态
	/// </summary>
	public class HttpExecuteRecord : IExecuteRecord
	{
		private readonly ILogger _logger;

		public HttpExecuteRecord(ILogger logger)
		{
			_logger = logger ?? throw new SpiderException($"{nameof(logger)} should not be null.");
		}

		/// <summary>
		/// 添加运行记录
		/// </summary>
		/// <param name="taskId">任务编号</param>
		/// <param name="name">任务名称</param>
		/// <param name="identity">任务标识</param>
		/// <returns>是否上报成功</returns>
		public bool Add(string taskId, string name, string identity)
		{
			if (string.IsNullOrWhiteSpace(taskId) || string.IsNullOrWhiteSpace(identity) || !Env.HubService)
			{
				return true;
			}

			try
			{
				var retryTimesPolicy = Policy.Handle<Exception>().Retry(10, (ex, count) =>
						{
							_logger?.Error($"Try to add execute record failed [{count}]: {ex}", LogUtil.Identity);
							Thread.Sleep(5000);
						});
				retryTimesPolicy.Execute(() =>
				{
					NetworkCenter.Current.Execute("executeRecord", () =>
					{
						var response = DotnetSpider.Downloader.Downloader.Default.GetAsync($"{Env.HubServiceTaskApiUrl}/{taskId}?action=increase").Result;
						response.EnsureSuccessStatusCode();
					});
				});
				return true;
			}
			catch (Exception e)
			{
				_logger?.Error($"Add execute record failed: {e}", identity);
				return false;
			}
		}

		/// <summary>
		/// 删除运行记录
		/// </summary>
		/// <param name="taskId">任务编号</param>
		/// <param name="name">任务名称</param>
		/// <param name="identity">任务标识</param>
		public void Remove(string taskId, string name, string identity)
		{
			if (string.IsNullOrWhiteSpace(taskId) || !Env.HubService)
			{
				return;
			}

			try
			{
				var retryTimesPolicy = Policy.Handle<Exception>().Retry(10, (ex, count) =>
				{
					_logger?.Error($"Try to remove execute record failed [{count}]: {ex}", identity);
					Thread.Sleep(5000);
				});
				retryTimesPolicy.Execute(() =>
				{
					NetworkCenter.Current.Execute("executeRecord", () =>
					{
						var response = DotnetSpider.Downloader.Downloader.Default.GetAsync($"{Env.HubServiceTaskApiUrl}/{taskId}?action=reduce").Result;
						response.EnsureSuccessStatusCode();
					});
				});
			}
			catch (Exception e)
			{
				_logger?.Error($"Remove execute record failed: {e}", identity);
			}
		}
	}
}
