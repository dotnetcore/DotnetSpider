using DotnetSpider.Common;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// URL调度队列, 实现广度优化或深度优化策略, 实现URL去重, 并且队列需要可被监控
	/// 考虑性能原因, 队列没有和ISpider的解耦, 因此同一个Scheduler不能被不同的Spider的使用
	/// </summary>
	public interface IScheduler : IDisposable, IMonitorable
	{
		bool IsDistributed { get; }

		/// <summary>
		/// 是否深度优先
		/// </summary>
		TraverseStrategy TraverseStrategy { get; set; }

		/// <summary>
		/// 遍历深度
		/// </summary>
		int Depth { get; set; }

		/// <summary>
		/// 添加请求对象到队列
		/// </summary>
		/// <param name="request">请求对象</param>
		/// <param name="shouldReserved">由各自的业务逻辑来确定是否需要重试</param>
		void Push(Request request, Func<Request, bool> shouldReserved);

		/// <summary>
		/// 取得一个需要处理的请求对象
		/// </summary>
		/// <returns>请求对象</returns>
		Request Poll();

		/// <summary>
		/// 批量导入
		/// </summary>
		/// <param name="requests">请求对象</param>
		void Reload(ICollection<Request> requests);

		/// <summary>
		/// 导出整个队列
		/// </summary>
		void Export();
	}
}
