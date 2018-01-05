using DotnetSpider.Core.Scheduler.Component;
using System.Collections.Generic;
using DotnetSpider.Core.Redial;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Remove duplicate urls and only push urls which are not duplicate.
	/// </summary>
	public abstract class DuplicateRemovedScheduler : Named, IScheduler
	{
		/// <summary>
		/// 去重器
		/// </summary>
		protected IDuplicateRemover DuplicateRemover { get; set; } = new HashSetDuplicateRemover();

		/// <summary>
		/// 爬虫对象
		/// </summary>
		protected ISpider Spider { get; set; }

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
		public abstract void Import(IEnumerable<Request> requests);

		/// <summary>
		/// 是否会使用互联网
		/// </summary>
		protected abstract bool UseInternet { get; set; }

		/// <summary>
		/// 剩余链接数
		/// </summary>
		public abstract long LeftRequestsCount { get; }

		/// <summary>
		/// 总的链接数
		/// </summary>
		public virtual long TotalRequestsCount => DuplicateRemover.TotalRequestsCount;

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
		public bool DepthFirst { get; set; } = true;

		/// <summary>
		/// 添加请求对象到队列
		/// </summary>
		/// <param name="request">请求对象</param>
		public void Push(Request request)
		{
			if (UseInternet)
			{
				NetworkCenter.Current.Execute("sch-push", () =>
				{
					DoPush(request);
				});
			}
			else
			{
				DoPush(request);
			}
		}

		/// <summary>
		/// 初始化队列
		/// </summary>
		/// <param name="spider">爬虫对象</param>
		public virtual void Init(ISpider spider)
		{
			if (Spider == null)
			{
				Spider = spider;
			}
			else
			{
				throw new SpiderException("Scheduler already init.");
			}
		}

		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		public abstract void ResetDuplicateCheck();

		/// <summary>
		/// 取得一个需要处理的请求对象
		/// </summary>
		/// <returns>请求对象</returns>
		public virtual Request Poll()
		{
			return null;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			DuplicateRemover.Dispose();
		}

		/// <summary>
		/// 导出整个队列
		/// </summary>
		public virtual void Export()
		{
		}

		/// <summary>
		/// 清空整个队列
		/// </summary>
		public virtual void Clear()
		{
		}

		/// <summary>
		/// 如果链接不是重复的就添加到队列中
		/// </summary>
		/// <param name="request">请求对象</param>
		protected virtual void PushWhenNoDuplicate(Request request)
		{
		}

		private bool ShouldReserved(Request request)
		{
			return request.CycleTriedTimes > 0 && request.CycleTriedTimes <= Spider.Site.CycleRetryTimes;
		}

		private void DoPush(Request request)
		{
			if (!DuplicateRemover.IsDuplicate(request) || ShouldReserved(request))
			{
				PushWhenNoDuplicate(request);
			}
		}
	}
}