using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DotnetSpider.Extensions;
using HWT;
using Microsoft.Extensions.Logging;

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
		private readonly ILogger<ProxyService> _logger;

		private readonly HashedWheelTimer _timer = new(TimeSpan.FromSeconds(1)
			, 100000);

		private readonly int _ignoreCount;
		private readonly int _reDetectCount;

		public ProxyService(IProxyValidator proxyValidator, ILogger<ProxyService> logger)
		{
			_proxyValidator = proxyValidator;
			_logger = logger;
			_queue = new ConcurrentQueue<ProxyEntry>();
			_dict = new ConcurrentDictionary<Uri, ProxyEntry>();
			_ignoreCount = 6;
			_reDetectCount = _ignoreCount / 2;
		}

		public async Task ReturnAsync(Uri proxy, HttpStatusCode statusCode)
		{
			if (_dict.TryGetValue(proxy, out var p))
			{
				// 若是返回成功，则直接把失败次数至为 0
				if (statusCode.IsSuccessStatusCode())
				{
					p.FailedNum = 0;
				}
				else
				{
					p.FailedNum += 1;
				}

				// 若是失败次数大于 ignoreCount，则把此代理从缓存中删除，不再使用
				if (p.FailedNum > _ignoreCount)
				{
					_dict.TryRemove(p.Uri, out _);
					return;
				}

				// 若是失败次数为 reDetectCount 的倍数则尝试重新测试此代理是否正常，若是测试不成功，则把此代理从缓存中删除，不再使用
				if ((p.FailedNum != 0 && p.FailedNum % _reDetectCount == 0) &&
				    !await _proxyValidator.IsAvailable(p.Uri))
				{
					_dict.TryRemove(p.Uri, out _);
					return;
				}

				_queue.Enqueue(p);
			}
		}

		public async Task<int> AddAsync(IEnumerable<Uri> proxies)
		{
			var cnt = 0;
			foreach (var proxy in proxies)
			{
				if (await _proxyValidator.IsAvailable(proxy) && _dict.TryAdd(proxy, new ProxyEntry(proxy)))
				{
					_logger.LogInformation($"proxy {proxy} is available");
					_queue.Enqueue(_dict[proxy]);
					cnt++;
				}
			}

			return cnt;
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
