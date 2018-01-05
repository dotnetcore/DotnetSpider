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
		/// <summary>
		/// 是否深度优先
		/// </summary>
		bool DepthFirst { get; set; }

		/// <summary>
		/// 初始化队列
		/// </summary>
		/// <param name="spider">爬虫对象</param>
		void Init(ISpider spider);

		/// <summary>
		/// 添加请求对象到队列
		/// </summary>
		/// <param name="request">请求对象</param>
		void Push(Request request);

		/// <summary>
		/// 取得一个需要处理的请求对象
		/// </summary>
		/// <returns>请求对象</returns>
		Request Poll();

		/// <summary>
		/// 批量导入
		/// </summary>
		/// <param name="requests">请求对象</param>
		void Import(IEnumerable<Request> requests);

		/// <summary>
		/// 导出整个队列
		/// </summary>
		void Export();

		/// <summary>
		/// 清空整个队列
		/// </summary>
		void Clear();
	}
}
