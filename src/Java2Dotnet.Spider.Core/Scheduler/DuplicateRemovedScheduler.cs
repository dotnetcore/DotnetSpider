
using System;
using Java2Dotnet.Spider.Core.Scheduler.Component;
using Java2Dotnet.Spider.Redial;
using System.Collections.Generic;
#if !NET_CORE
using log4net;
#else
using Java2Dotnet.Spider.JLog;
using System.Runtime.CompilerServices;
#endif

namespace Java2Dotnet.Spider.Core.Scheduler
{
	/// <summary>
	/// Remove duplicate urls and only push urls which are not duplicate.
	/// </summary>
	public abstract class DuplicateRemovedScheduler : IScheduler
	{
#if !NET_CORE
		protected static readonly ILog Logger = LogManager.GetLogger(typeof(DuplicateRemovedScheduler));
#else
		protected static readonly ILog Logger = LogManager.GetLogger();
#endif


		protected IDuplicateRemover DuplicateRemover { get; set; } = new HashSetDuplicateRemover();


		public void Push(Request request, ISpider spider)
		{
			lock (this)
			{

				RedialManagerUtils.Execute("scheduler-push", () =>
				{
					DoPush(request, spider);
				});
			}

		}

		public virtual void Init(ISpider spider)
		{
		}

		public abstract void ResetDuplicateCheck(ISpider spider);

		public virtual Request Poll(ISpider spider)
		{
			return null;
		}

		protected virtual void PushWhenNoDuplicate(Request request, ISpider spider)
		{
		}

		/// <summary>
		/// 用于如果URL执行失败, 重新添加回TargetUrls时因Hash而不能重新加入队列的问题
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		private bool ShouldReserved(Request request)
		{
			var cycleTriedTimes = (int?)request.GetExtra(Request.CycleTriedTimes);

			return cycleTriedTimes > 0;
		}

		private void DoPush(Request request, ISpider spider)
		{
			if (!DuplicateRemover.IsDuplicate(request, spider) || ShouldReserved(request))
			{
				PushWhenNoDuplicate(request, spider);
			}
		}

		public virtual void Dispose()
		{
			DuplicateRemover.Dispose();
		}

		public abstract void Load(HashSet<Request> requests, ISpider spider);

		public abstract HashSet<Request> ToList(ISpider spider);
	}
}