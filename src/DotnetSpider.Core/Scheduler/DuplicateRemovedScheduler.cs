using DotnetSpider.Core.Scheduler.Component;
using System.Collections.Generic;
using DotnetSpider.Core.Redial;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Remove duplicate urls and only push urls which are not duplicate.
	/// </summary>
	public abstract class DuplicateRemovedScheduler : BaseScheduler
	{
		/// <summary>
		/// 去重器
		/// </summary>
		protected IDuplicateRemover DuplicateRemover { get; set; } = new HashSetDuplicateRemover();

		/// <summary>
		/// 总的链接数
		/// </summary>
		public override long TotalRequestsCount => DuplicateRemover.TotalRequestsCount;


		/// <summary>
		/// Reset duplicate check.
		/// </summary>
		public abstract void ResetDuplicateCheck();


		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			DuplicateRemover.Dispose();
		}

		/// <summary>
		/// 如果链接不是重复的就添加到队列中
		/// </summary>
		/// <param name="request">请求对象</param>
		protected abstract void PushWhenNoDuplicate(Request request);

		protected override void DoPush(Request request)
		{
			if (!DuplicateRemover.IsDuplicate(request) || ShouldReserved(request))
			{
				PushWhenNoDuplicate(request);
			}
		}
	}
}