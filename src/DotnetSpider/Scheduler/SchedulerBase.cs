using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler.Component;

namespace DotnetSpider.Scheduler
{
	public abstract class SchedulerBase : IScheduler
	{
		private SpinLock _spinLock;

		protected readonly IDuplicateRemover DuplicateRemover;

		private readonly IRequestHasher RequestHasher;
		protected SchedulerBase(IDuplicateRemover duplicateRemover, IRequestHasher requestHasher)
		{
			DuplicateRemover = duplicateRemover;
			RequestHasher = requestHasher;
		}

		/// <summary>
		/// 重置去重器
		/// </summary>
		public virtual void ResetDuplicateCheck()
		{
			DuplicateRemover.ResetDuplicateCheck();
		}

		/// <summary>
		/// 如果请求未重复就添加到队列中
		/// </summary>
		/// <param name="request">请求</param>
		protected abstract Task PushWhenNoDuplicate(Request request);

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			DuplicateRemover.Dispose();
		}

		/// <summary>
		/// 队列中的总请求个数
		/// </summary>
		public long Total => DuplicateRemover.Total;

		/// <summary>
		/// 从队列中取出指定爬虫的指定个数请求
		/// </summary>
		/// <param name="count">出队数</param>
		/// <returns>请求</returns>
		protected abstract Task<IEnumerable<Request>> ImplDequeueAsync(int count = 1);

		public async Task<IEnumerable<Request>> DequeueAsync(int count = 1)
		{
			var locker = false;

			try
			{
				//申请获取锁
				_spinLock.Enter(ref locker);

				return await ImplDequeueAsync(count);
			}
			finally
			{
				//工作完毕，或者发生异常时，检测一下当前线程是否占有锁，如果咱有了锁释放它
				//以避免出现死锁的情况
				if (locker)
				{
					_spinLock.Exit();
				}
			}
		}

		/// <summary>
		/// 请求入队
		/// </summary>
		/// <param name="requests">请求</param>
		/// <returns>入队个数</returns>
		public async Task<int> EnqueueAsync(IEnumerable<Request> requests)
		{
			var count = 0;
			foreach (var request in requests)
			{
				request.Hash = RequestHasher.ComputeHash(request);
				if (!await DuplicateRemover.IsDuplicateAsync(request))
				{
					await PushWhenNoDuplicate(request);
					count++;
				}
			}

			return count;
		}
	}
}
