using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DotnetSpider.Http;
using HWT;

namespace DotnetSpider.Infrastructure
{
	public class RequestedQueue : IDisposable
	{
		private readonly Dictionary<string, Request> _dict;

		private readonly HashedWheelTimer _timer;

		private readonly List<Request> _queue;

		public RequestedQueue()
		{
			_dict = new Dictionary<string, Request>();
			_queue = new List<Request>();
			_timer = new HashedWheelTimer(TimeSpan.FromSeconds(1)
				, ticksPerWheel: 100000
				, maxPendingTimeouts: 0);
		}

		public int Count => _dict.Count;

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool Enqueue(Request request)
		{
			if (request.Timeout <= 2)
			{
				throw new SpiderException("Timeout should not less than 2 second");
			}

			if (!_dict.ContainsKey(request.Hash))
			{
				_dict.Add(request.Hash, request);
				_timer.NewTimeout(new TimeoutTask(this, request.Hash),
					TimeSpan.FromSeconds(request.Timeout));
				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public Request Dequeue(string hash)
		{
			var request = _dict[hash];
			_dict.Remove(hash);
			return request;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public Request[] GetAllTimeoutList()
		{
			var data = _queue.ToArray();
			_queue.Clear();
			return data;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void Timeout(string hash)
		{
			if (_dict.ContainsKey(hash))
			{
				var request = _dict[hash];
				_queue.Add(request);
				_dict.Remove(hash);
			}
		}

		private class TimeoutTask : ITimerTask
		{
			private readonly string _hash;
			private readonly RequestedQueue _requestedQueue;

			public TimeoutTask(RequestedQueue requestedQueue, string hash)
			{
				_hash = hash;
				_requestedQueue = requestedQueue;
			}

			public Task RunAsync(ITimeout timeout)
			{
				_requestedQueue.Timeout(_hash);
				return Task.CompletedTask;
			}
		}

		public void Dispose()
		{
			_dict.Clear();
			_queue.Clear();
			_timer.Dispose();
		}
	}
}
