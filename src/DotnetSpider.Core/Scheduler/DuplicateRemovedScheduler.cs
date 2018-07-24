using DotnetSpider.Common;
using DotnetSpider.Core.Scheduler.Component;
using DotnetSpider.Downloader;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Scheduler
{
	public abstract class DuplicateRemovedScheduler : Named, IScheduler
	{
		private int _depth = int.MaxValue;

		/// <summary>
		/// 去重器
		/// </summary>
		protected IDuplicateRemover DuplicateRemover { get; set; } = new HashSetDuplicateRemover();

		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		public abstract void ResetDuplicateCheck();

		/// <summary>
		/// 总的链接数
		/// </summary>
		public virtual long TotalRequestsCount => DuplicateRemover.TotalRequestsCount;

		/// <summary>
		/// 是否是分布式调度器
		/// </summary>
		public abstract bool IsDistributed { get; }

		/// <summary>
		/// 采集成功的链接数加 1
		/// </summary>
		public abstract void IncreaseSuccessCount();

		/// <summary>
		/// 采集失败的次数加 1
		/// </summary>
		public abstract void IncreaseErrorCount();

		/// <summary>
		/// 批量导入
		/// </summary>
		/// <param name="requests">请求对象</param>
		public abstract void Reload(ICollection<Request> requests);

		/// <summary>
		/// 如果链接不是重复的就添加到队列中
		/// </summary>
		/// <param name="request">请求对象</param>
		protected abstract void PushWhenNoDuplicate(Request request);

		/// <summary>
		/// 是否会使用互联网
		/// </summary>
		protected abstract bool UseInternet { get; set; }

		/// <summary>
		/// 剩余链接数
		/// </summary>
		public abstract long LeftRequestsCount { get; }

		/// <summary>
		/// 采集成功的链接数
		/// </summary>
		public abstract long SuccessRequestsCount { get; }

		/// <summary>
		/// 采集失败的次数, 不是链接数, 如果一个链接采集多次都失败会记录多次
		/// </summary>
		public abstract long ErrorRequestsCount { get; }

		/// <summary>
		/// 是否深度优先
		/// </summary>
		public TraverseStrategy TraverseStrategy { get; set; } = TraverseStrategy.Dfs;

		public int Depth
		{
			get => _depth;
			set
			{
				if (value <= 0)
				{
					throw new ArgumentException("Depth should be greater than 0.");
				}

				_depth = value;
			}
		}

		/// <summary>
		/// 添加请求对象到队列
		/// </summary>
		/// <param name="request">请求对象</param>
		/// <param name="shouldReserved">是否需要重试判断方法</param>
		public void Push(Request request, Func<Request, bool> shouldReserved = null)
		{
			var action = new Action(() =>
			{
				if (!DuplicateRemover.IsDuplicate(request) || shouldReserved != null && shouldReserved(request))
				{
					PushWhenNoDuplicate(request);
				}
			});
			if (UseInternet)
			{
				NetworkCenter.Current.Execute("sch-push", action);
			}
			else
			{
				action();
			}
		}

		/// <summary>
		/// 取得一个需要处理的请求对象
		/// </summary>
		/// <returns>请求对象</returns>
		public abstract Request Poll();

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			DuplicateRemover?.Dispose();
		}

		/// <summary>
		/// 导出整个队列
		/// </summary>
		public virtual void Export()
		{
		}
	}
}