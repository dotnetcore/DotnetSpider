using DotnetSpider.Core.Scheduler.Component;
using System.Collections.Generic;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Scheduler
{
	/// <summary>
	/// Remove duplicate urls and only push urls which are not duplicate.
	/// </summary>
	public abstract class DuplicateRemovedScheduler : Named, IScheduler
	{
		protected IDuplicateRemover DuplicateRemover { get; set; } = new HashSetDuplicateRemover();
		protected ISpider Spider { get; set; }

		public bool DepthFirst { get; set; } = true;

		public virtual bool IsExited { get; set; }
 
		public void Push(Request request)
		{
			lock (this)
			{
				NetworkCenter.Current.Execute("sp", () =>
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
		/// �������URLִ��ʧ��, ������ӻ�TargetUrlsʱ��Hash���������¼�����е�����
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		private bool ShouldReserved(Request request)
		{
			var cycleTriedTimes = request.GetExtra(Request.CycleTriedTimes);
			var resultEmptyTriedTimes = request.GetExtra(Request.ResultIsEmptyTriedTimes);
			if (cycleTriedTimes == null && resultEmptyTriedTimes == null)
			{
				return false;
			}
			else
			{
				return (cycleTriedTimes != null && cycleTriedTimes > 0) || (resultEmptyTriedTimes != null && resultEmptyTriedTimes > 0);
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
			IsExited = true;
		}

		public abstract void Import(HashSet<Request> requests);

		public abstract HashSet<Request> ToList();

		public abstract long GetLeftRequestsCount();

		public abstract long GetTotalRequestsCount();

		public abstract long GetSuccessRequestsCount();

		public abstract long GetErrorRequestsCount();

		public abstract void IncreaseSuccessCounter();

		public abstract void IncreaseErrorCounter();

		public virtual void Export()
		{
		}

		public virtual void Clean()
		{
		}
	}
}