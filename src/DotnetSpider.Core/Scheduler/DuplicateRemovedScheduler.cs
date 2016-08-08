
using System;
using DotnetSpider.Core.Scheduler.Component;
using System.Collections.Generic;
#if !NET_CORE
#else
using System.Runtime.CompilerServices;
#endif

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Remove duplicate urls and only push urls which are not duplicate.
	/// </summary>
	public abstract class DuplicateRemovedScheduler : IScheduler
	{
		protected IDuplicateRemover DuplicateRemover { get; set; } = new HashSetDuplicateRemover();
		public ISpider Spider { get; protected set; }

		public void Push(Request request)
		{
			lock (this)
			{
				NetworkProxyManager.Current.Execute("sp", () =>
				{
					DoPush(request);
				});
			}
		}

		public virtual void Init(ISpider spider)
		{
			Spider = spider;
		}

		public abstract void ResetDuplicateCheck();

		public virtual Request Poll()
		{
			return null;
		}

		protected virtual void PushWhenNoDuplicate(Request request)
		{
		}

		/// <summary>
		/// 用于如果URL执行失败, 重新添加回TargetUrls时因Hash而不能重新加入队列的问题
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		private bool ShouldReserved(Request request)
		{
			var cycleTriedTimes = request.GetExtra(Request.CycleTriedTimes);
			if (cycleTriedTimes == null)
			{
				return false;
			}
			else
			{
				return cycleTriedTimes > 0;
			}
		}

		private void DoPush(Request request)
		{
			if (!DuplicateRemover.IsDuplicate(request) || ShouldReserved(request))
			{
				PushWhenNoDuplicate(request);
			}
		}

		public virtual void Dispose()
		{
			DuplicateRemover.Dispose();
		}

		public abstract void Load(HashSet<Request> requests);

		public abstract HashSet<Request> ToList();

		public virtual void Clear()
		{
			DuplicateRemover.ResetDuplicateCheck();
		}
	}
}