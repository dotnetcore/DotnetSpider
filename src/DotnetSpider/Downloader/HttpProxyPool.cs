using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Common;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 代理池
	/// </summary>
	public class HttpProxyPool : IHttpProxyPool
	{
		private readonly IProxySupplier _supplier;
		private readonly List<Proxy> _proxyQueue = new List<Proxy>();
		private readonly ConcurrentDictionary<string, Proxy> _proxies = new ConcurrentDictionary<string, Proxy>();
		private bool _isDispose;
		private readonly int _reuseInterval;
		private readonly object _proxyQueueLocker = new object();
		private readonly ILogger _logger;
		public IProxyValidator ProxyValidator { get; set; } = new DefaultProxyValidator();

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="supplier">代理提供接口</param>
		/// <param name="reuseInterval">代理不被再次使用的间隔</param>
		public HttpProxyPool(ILogger logger, IProxySupplier supplier, int reuseInterval = 500)
		{
			_logger = logger;
			_supplier = supplier ?? throw new SpiderException($"{nameof(supplier)} is null.");

			_reuseInterval = reuseInterval;
			if (ProxyValidator != null)
			{
				ThreadPool.QueueUserWorkItem(RefreshProxies);
			}
		}

		/// <summary>
		/// 从代理池中取一个代理
		/// </summary>
		/// <returns>代理</returns>
		public WebProxy GetProxy()
		{
			for (var i = 0; i < 3600; ++i)
			{
				lock (_proxyQueueLocker)
				{
					var proxy = GetFirstAvailableProxy();
					if (proxy != null)
					{
						proxy.SetLastBorrowTime(DateTimeHelper.GetCurrentUnixTimeNumber());
						_proxyQueue.Remove(proxy);
						return proxy.GetWebProxy();
					}
				}

				Thread.Sleep(1000);
			}

			throw new SpiderException("There is no available proxy.");
		}

		/// <summary>
		/// 把代理返回给代理池
		/// </summary>
		/// <param name="proxy">代理</param>
		/// <param name="statusCode">通过此代理请求数据后的返回状态</param>
		public async Task ReturnProxy(WebProxy proxy, HttpStatusCode statusCode)
		{
			if (proxy == null)
			{
				return;
			}

			var key = $"{proxy.Address.Host}:{proxy.Address.Port}";
			if (!_proxies.ContainsKey(key))
			{
				return;
			}

			var p = _proxies[key];
			switch (statusCode)
			{
				case HttpStatusCode.OK:
					p.SetFailedNum(0);
					p.SetReuseTime(_reuseInterval);
					p.RecordResponse();
					break;
				case HttpStatusCode.Forbidden:
					p.Fail();
					p.SetReuseTime(_reuseInterval * p.FailedNum);
					break;
				case HttpStatusCode.NotFound:
					p.Fail();
					p.SetReuseTime(_reuseInterval * p.FailedNum);
					break;
				default:
					p.Fail();
					p.SetReuseTime(_reuseInterval * p.FailedNum);
					break;
			}

			if (p.FailedNum > 6)
			{
				return;
			}

			if (ProxyValidator != null && p.FailedNum % 3 == 0 && await ProxyValidator.IsAvailable(proxy))
			{
				return;
			}

			lock (_proxyQueueLocker)
			{
				_proxyQueue.Add(p);
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_isDispose = true;
		}

		private Proxy GetFirstAvailableProxy()
		{
			var currentUnixTimeNumber = DateTimeHelper.GetCurrentUnixTimeNumber();
			foreach (var proxy in _proxyQueue)
			{
				if (currentUnixTimeNumber - proxy.GetLastUseTime() > _reuseInterval)
				{
					return proxy;
				}
			}

			return null;
		}

		private void RefreshProxies(object threadState)
		{
			if (ProxyValidator == null)
			{
				return;
			}

			var threadCommonPool = new LimitedConcurrencyThreadPool(4);
			while (!_isDispose)
			{
				if (_proxyQueue.Count < 50)
				{
					var proxies = _supplier.GetProxies().GetAwaiter().GetResult();

					foreach (var proxy in proxies)
					{
						threadCommonPool.QueueUserWork(async item =>
						{
							if (!_proxies.ContainsKey(item.Key))
							{
								if (await ProxyValidator.IsAvailable(item.Value.WebProxy))
								{
									_logger.LogInformation("");
									item.Value.SetFailedNum(0);
									item.Value.SetReuseTime(_reuseInterval);

									lock (_proxyQueueLocker)
									{
										_proxyQueue.Add(item.Value);
									}

									_proxies.TryAdd(item.Key, item.Value);
									_logger.LogInformation($"Acquired available proxy {proxy.Value.WebProxy.Address}");
								}
							}
						}, proxy);
					}
				}

				Thread.Sleep(30000);
			}
		}
	}
}
