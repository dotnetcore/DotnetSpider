using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DotnetSpider.Downloader;
using DotnetSpider.Scheduler.Component;

namespace DotnetSpider.Scheduler
{
	public abstract class DuplicateRemovedScheduler : IScheduler
	{
		/// <summary>
		/// 重置去重器
		/// </summary>
		public abstract void ResetDuplicateCheck();

		/// <summary>
		/// 如果请求未重复就添加到队列中
		/// </summary>
		/// <param name="request">请求</param>
		protected abstract void PushWhenNoDuplicate(Request request);

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			DuplicateRemover?.Dispose();
		}

		/// <summary>
		/// 去重器
		/// </summary>
		protected IDuplicateRemover DuplicateRemover { get; set; } = new HashSetDuplicateRemover();

		/// <summary>
		/// 队列中的总请求个数
		/// </summary>
		public int Total => DuplicateRemover.Total;

		/// <summary>
		/// 从队列中取出指定爬虫的指定个数请求
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <param name="count">出队数</param>
		/// <returns>请求</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public abstract Request[] Dequeue(string ownerId, int count = 1);

		/// <summary>
		/// 请求入队
		/// </summary>
		/// <param name="requests">请求</param>
		/// <returns>入队个数</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public int Enqueue(IEnumerable<Request> requests)
		{
			int count = 0;
			foreach (var request in requests)
			{
				request.ComputeHash();
				if (!DuplicateRemover.IsDuplicate(request))
				{
					PushWhenNoDuplicate(request);
					count++;
				}
			}

			return count;
		}
	}
}