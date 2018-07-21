using DotnetSpider.Common;
using DotnetSpider.Core.Scheduler.Component;
using System;

namespace DotnetSpider.Core.Scheduler
{
	public class QueueScheduler : QueueDuplicateRemovedScheduler
	{
		class FakeDuplicateRemover : IDuplicateRemover
		{
			private readonly AutomicLong _totalCounter = new AutomicLong(0);

			public long TotalRequestsCount => _totalCounter.Value;

			public void Dispose()
			{
			}

			public bool IsDuplicate(Request request)
			{
				_totalCounter.Inc();
				return false;
			}

			public void ResetDuplicateCheck()
			{
			}
		}

		public QueueScheduler()
		{
			DuplicateRemover = new FakeDuplicateRemover();
		}

		public override void ResetDuplicateCheck()
		{
			throw new NotImplementedException("None duplicate removed scheduler can not reset duplicate check.");
		}
	}
}