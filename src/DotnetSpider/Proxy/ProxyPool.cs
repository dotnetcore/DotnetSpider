using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using HWT;

namespace DotnetSpider.Proxy
{
	public class ProxyPool : IProxyPool
	{
		private readonly ConcurrentQueue<HttpProxy> _queue;
		private readonly ConcurrentDictionary<string, HttpProxy> _dict;
		private readonly IProxyValidator _proxyValidator;

		private readonly HashedWheelTimer _timer = new HashedWheelTimer(TimeSpan.FromSeconds(1)
			, ticksPerWheel: 100000
			, maxPendingTimeouts: 0);

		public ProxyPool(IProxyValidator proxyValidator)
		{
			_proxyValidator = proxyValidator;
			_queue = new ConcurrentQueue<HttpProxy>();
			_dict = new ConcurrentDictionary<string, HttpProxy>();
		}

		public async Task<HttpProxy> GetAsync(int seconds)
		{
			var waitCount = seconds * 10;
			for (int i = 0; i < waitCount; ++i)
			{
				var proxy = Get();
				if (proxy != null)
				{
					return proxy;
				}

				await Task.Delay(100);
			}

			return null;
		}

		public async Task ReturnAsync(HttpProxy proxy, HttpStatusCode statusCode)
		{
			if (proxy == null)
			{
				return;
			}

			if (_dict.TryGetValue(proxy.Uri, out var p))
			{
				if (statusCode.IsSuccessStatusCode())
				{
					p.FailedNum = 0;
				}
				else
				{
					p.FailedNum += 1;
				}

				if (p.FailedNum > 6)
				{
					_dict.TryRemove(p.Uri, out _);
					return;
				}

				if (_proxyValidator != null && p.FailedNum % 3 == 0 && await _proxyValidator.IsAvailable(proxy))
				{
					_dict.TryRemove(p.Uri, out _);
					return;
				}

				_queue.TryDequeue(out p);
			}
		}

		public async Task LoadAsync(IEnumerable<HttpProxy> proxies)
		{
			foreach (var proxy in proxies)
			{
				if (await _proxyValidator.IsAvailable(proxy) && _dict.TryAdd(proxy.Uri, proxy))
				{
					_queue.Enqueue(proxy);
				}
			}
		}

		private HttpProxy Get()
		{
			if (_queue.TryDequeue(out var proxy))
			{
				_timer.NewTimeout(new ReturnProxyTask(this, proxy), TimeSpan.FromSeconds(30));
				return proxy;
			}
			else
			{
				return null;
			}
		}

		private class ReturnProxyTask : ITimerTask
		{
			private readonly HttpProxy _httpProxy;
			private readonly IProxyPool _pool;

			public ReturnProxyTask(IProxyPool pool, HttpProxy httpProxy)
			{
				_pool = pool;
				_httpProxy = httpProxy;
			}

			public async Task RunAsync(ITimeout timeout)
			{
				await _pool.ReturnAsync(_httpProxy, HttpStatusCode.OK);
			}
		}
	}
}
