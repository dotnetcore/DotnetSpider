using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler.Component;

namespace DotnetSpider.Scheduler
{
	/// <summary>
	/// 基于内存的深度优先调度(不去重 URL)
	/// </summary>
	public class QueueDfsScheduler : SchedulerBase
	{
		private readonly List<Request> _requests =
			new List<Request>();

		/// <summary>
		/// 构造方法
		/// </summary>
		public QueueDfsScheduler(IRequestHasher requestHasher) : base(new FakeDuplicateRemover(), requestHasher)
		{
		}

		public override void Dispose()
		{
			_requests.Clear();
			base.Dispose();
		}

		/// <summary>
		/// 如果请求未重复就添加到队列中
		/// </summary>
		/// <param name="request">请求</param>
		protected override Task PushWhenNoDuplicate(Request request)
		{
			_requests.Add(request);
			return Task.CompletedTask;
		}

		/// <summary>
		/// 从队列中取出指定爬虫的指定个数请求
		/// </summary>
		/// <param name="count">出队数</param>
		/// <returns>请求</returns>
		protected override Task<IEnumerable<Request>> ImplDequeueAsync(int count = 1)
		{
			var dequeueCount = count;
			int start;
			if (_requests.Count < count)
			{
				dequeueCount = _requests.Count;
				start = 0;
			}
			else
			{
				start = _requests.Count - dequeueCount - 1;
			}

			var requests = new List<Request>();
			for (var i = _requests.Count - 1; i >= start; --i)
			{
				requests.Add(_requests[i]);
			}

			if (dequeueCount > 0)
			{
				_requests.RemoveRange(start, dequeueCount);
			}

			return Task.FromResult(requests.Select(x => x.Clone()));
		}
	}
}
