using DotnetSpider.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Core.Scheduler
{
	public class QueueScheduler : BaseScheduler
	{
		private readonly object _lock = new object();
		private List<Request> _queue = new List<Request>();
		private readonly AutomicLong _successCounter = new AutomicLong(0);
		private readonly AutomicLong _errorCounter = new AutomicLong(0);
		private readonly AutomicLong _totalCounter = new AutomicLong(0);

		/// <summary>
		/// 是否会使用互联网
		/// </summary>
		protected override bool UseInternet { get; set; } = false;

		/// <summary>
		/// 剩余链接数
		/// </summary>
		public override long LeftRequestsCount
		{
			get
			{
				lock (_lock)
				{
					return _queue.Count;
				}
			}
		}

		public override long TotalRequestsCount => _totalCounter.Value;

		/// <summary>
		/// 采集成功的链接数
		/// </summary>
		public override long SuccessRequestsCount => _successCounter.Value;

		/// <summary>
		/// 采集失败的次数, 不是链接数, 如果一个链接采集多次都失败会记录多次
		/// </summary>
		public override long ErrorRequestsCount => _errorCounter.Value;

		public override bool IsDistributed => false;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			lock (_lock)
			{
				_successCounter.Set(0);
				_errorCounter.Set(0);
				_queue.Clear();
			}
		}

		/// <summary>
		/// 采集失败的次数加 1
		/// </summary>
		public override void IncreaseErrorCount()
		{
			_errorCounter.Inc();
		}

		/// <summary>
		/// 采集成功的链接数加 1
		/// </summary>
		public override void IncreaseSuccessCount()
		{
			_successCounter.Inc();
		}

		/// <summary>
		/// 取得一个需要处理的请求对象
		/// </summary>
		/// <returns>请求对象</returns>
		public override Request Poll()
		{
			lock (_lock)
			{
				if (_queue.Count == 0)
				{
					return null;
				}
				else
				{
					Request request;

					switch (TraverseStrategy)
					{
						case TraverseStrategy.Dfs:
						{
							request = _queue.Last();
							_queue.RemoveAt(_queue.Count - 1);
							break;
						}
						case TraverseStrategy.Bfs:
						{
							request = _queue.First();
							_queue.RemoveAt(0);
							break;
						}
						default:
						{
							throw new NotImplementedException();
						}
					}

					return request;
				}
			}
		}

		protected override void ImplPush(Request request)
		{
			if (ShouldReserved(request))
			{
				_totalCounter.Inc();
				lock (_lock)
				{
					_queue.Add(request);
				}
			}
		}

		protected override bool ShouldReserved(Request request)
		{
			return request.CycleTriedTimes == 0 ||
			       (request.CycleTriedTimes > 0 && request.CycleTriedTimes <= Spider.Site.CycleRetryTimes);
		}

		/// <summary>
		/// 批量导入
		/// </summary>
		/// <param name="requests">请求对象</param>
		public override void Import(IEnumerable<Request> requests)
		{
			if (requests == null)
			{
				return;
			}

			lock (_lock)
			{
				_queue.Clear();
				int count = 0;
				foreach (var request in requests)
				{
					count++;
					_queue.Add(request);
				}

				_totalCounter.Set(count);
			}
		}
	}
}