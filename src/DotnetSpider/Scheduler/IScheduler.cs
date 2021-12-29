using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetSpider.Http;

namespace DotnetSpider.Scheduler
{
	/// <summary>
	/// 调度器接口
	/// </summary>
	public interface IScheduler : IDisposable
	{
		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="spiderId"></param>
		Task InitializeAsync(string spiderId);

		/// <summary>
		/// 从队列中取出指定爬虫的指定个数请求
		/// </summary>
		/// <param name="count">出队数</param>
		/// <returns>请求</returns>
		Task<IEnumerable<Request>> DequeueAsync(int count = 1);

		/// <summary>
		/// 请求入队
		/// </summary>
		/// <param name="requests">请求</param>
		/// <returns>入队个数</returns>
		Task<int> EnqueueAsync(IEnumerable<Request> requests);

		/// <summary>
		/// 队列中的总请求个数
		/// </summary>
		Task<long> GetTotalAsync();

		/// <summary>
		/// 重置
		/// </summary>
		/// <returns></returns>
		Task ResetDuplicateCheckAsync();

		/// <summary>
		/// 标记请求成功
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		Task SuccessAsync(Request request);

		/// <summary>
		/// 标记请求失败
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		Task FailAsync(Request request);
	}
}
