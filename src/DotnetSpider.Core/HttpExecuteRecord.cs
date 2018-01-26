using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Redial;
using Newtonsoft.Json;
using Polly;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 通过Http向企业服务上报运行状态
	/// </summary>
	public class HttpExecuteRecord : IExecuteRecord
	{
		private static readonly ILogger Logger = DLog.GetLogger();

		/// <summary>
		/// 添加运行记录
		/// </summary>
		/// <param name="taskId">任务编号</param>
		/// <param name="name">任务名称</param>
		/// <param name="identity">任务标识</param>
		/// <returns>是否上报成功</returns>
		public bool Add(string taskId, string name, string identity)
		{
			if (string.IsNullOrWhiteSpace(taskId) || string.IsNullOrWhiteSpace(identity) || !Env.EnterpiseService)
			{
				return true;
			}

			try
			{
				var retryTimesPolicy = Policy.Handle<Exception>().Retry(10, (ex, count) =>
						{
							Logger.NLog($"Try to add execute record failed [{count}]: {ex}", Level.Error);
							Thread.Sleep(5000);
						});
				retryTimesPolicy.Execute(() =>
				{
					NetworkCenter.Current.Execute("executeRecord", () =>
					{
						var response = HttpSender.Client.GetAsync($"{Env.EnterpiseServiceTaskApiUrl}/{taskId}?action=increase").Result;
						response.EnsureSuccessStatusCode();
					});
				});
				return true;
			}
			catch (Exception e)
			{
				Logger.NLog($"Add execute record failed: {e}", Level.Error);
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
			if (string.IsNullOrWhiteSpace(taskId) || !Env.EnterpiseService)
			{
				return;
			}

			try
			{
				var retryTimesPolicy = Policy.Handle<Exception>().Retry(10, (ex, count) =>
				{
					Logger.NLog($"Try to remove execute record failed [{count}]: {ex}", Level.Error);
					Thread.Sleep(5000);
				});
				retryTimesPolicy.Execute(() =>
				{
					NetworkCenter.Current.Execute("executeRecord", () =>
					{
						var response = HttpSender.Client.GetAsync($"{Env.EnterpiseServiceTaskApiUrl}/{taskId}?action=reduce").Result;
						response.EnsureSuccessStatusCode();
					});
				});
			}
			catch (Exception e)
			{
				Logger.NLog($"Remove execute record failed: {e}", Level.Error);
			}
		}
	}
}
