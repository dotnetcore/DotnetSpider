using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using HWT;

namespace DotnetSpider.Proxy
{
	public class ProxyService : IProxyService
	{
		private class ProxyEntry
		{
			public Uri Uri { get; private set; }

			/// <summary>
			/// 使用此代理下载数据的失败次数
			/// </summary>
			public int FailedNum { get; set; }

			public ProxyEntry(Uri uri)
			{
				Uri = uri;
			}
		}

		private readonly ConcurrentQueue<ProxyEntry> _queue;
		private readonly ConcurrentDictionary<Uri, ProxyEntry> _dict;
		private readonly IProxyValidator _proxyValidator;

		private readonly HashedWheelTimer _timer = new HashedWheelTimer(TimeSpan.FromSeconds(1)
			, 100000
			, 0);

		public ProxyService(IProxyValidator proxyValidator)
		{
			_proxyValidator = proxyValidator;
			_queue = new ConcurrentQueue<ProxyEntry>();
			_dict = new ConcurrentDictionary<Uri, ProxyEntry>();
		}

		public async Task ReturnAsync(Uri proxy, HttpStatusCode statusCode)
		{
			if (_dict.TryGetValue(proxy, out var p))
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

				if (_proxyValidator != null && p.FailedNum % 3 == 0 && await _proxyValidator.IsAvailable(p.Uri))
				{
					_dict.TryRemove(p.Uri, out _);
					return;
				}

				_queue.TryDequeue(out p);
			}
		}

		public async Task AddAsync(IEnumerable<Uri> proxies)
		{
			foreach (var proxy in proxies)
			{
				if (await _proxyValidator.IsAvailable(proxy) && _dict.TryAdd(proxy, new ProxyEntry(proxy)))
				{
					_queue.Enqueue(_dict[proxy]);
				}
			}
		}

		public async Task<Uri> GetAsync(int seconds)
		{
			var waitCount = seconds * 10;
			for (var i = 0; i < waitCount; ++i)
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

		public Uri Get()
		{
			if (_queue.TryDequeue(out var proxy))
			{
				_timer.NewTimeout(new ReturnProxyTask(this, proxy.Uri), TimeSpan.FromSeconds(30));
				return proxy.Uri;
			}
			else
			{
				return null;
			}
		}

		private class ReturnProxyTask : ITimerTask
		{
			private readonly Uri _proxy;
			private readonly IProxyService _pool;

			public ReturnProxyTask(IProxyService pool, Uri proxy)
			{
				_pool = pool;
				_proxy = proxy;
			}

			public async Task RunAsync(ITimeout timeout)
			{
				await _pool.ReturnAsync(_proxy, HttpStatusCode.OK);
			}
		}
	}
}
