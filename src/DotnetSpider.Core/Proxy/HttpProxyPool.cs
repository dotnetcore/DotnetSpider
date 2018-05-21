using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Proxy
{
	/// <summary>
	/// 代理池
	/// </summary>
	public class HttpProxyPool : IHttpProxyPool
	{
		private readonly IProxySupplier _supplier;
		private readonly List<Proxy> _proxyQueue = new List<Proxy>();
		private readonly ConcurrentDictionary<string, Proxy> _allProxy = new ConcurrentDictionary<string, Proxy>();
		private bool _isDispose;
		private readonly int _reuseInterval;
		private readonly object _locker = new object();

		public IProxyValidator ProxyValidator { get; set; } = new DefaultProxyValidator();

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="supplier">代理提供接口</param>
		/// <param name="reuseInterval">代理不被再次使用的间隔</param>
		public HttpProxyPool(IProxySupplier supplier, int reuseInterval = 500)
		{
			_supplier = supplier ?? throw new SpiderException("IProxySupplier should not be null");

			_reuseInterval = reuseInterval;

			Task.Factory.StartNew(() =>
			{
				while (!_isDispose)
				{
					if (_proxyQueue.Count < 50)
					{
						RefreshProxies();
					}
					Thread.Sleep(30000);
				}
			});
		}

		/// <summary>
		/// 从代理池中取一个代理
		/// </summary>
		/// <returns>代理</returns>
		public UseSpecifiedUriWebProxy GetProxy()
		{
			for (int i = 0; i < 3600; ++i)
			{
				lock (_locker)
				{
					var proxy = _proxyQueue.FirstOrDefault(p => DateTimeUtil.GetCurrentUnixTimeNumber() - p.GetLastUseTime() > _reuseInterval);
					if (proxy != null)
					{
						proxy.SetLastBorrowTime(DateTimeUtil.GetCurrentUnixTimeNumber());
						_proxyQueue.Remove(proxy);
						return proxy.GetWebProxy();
					}
				}
				Thread.Sleep(1000);
			}

			throw new SpiderException("Get proxy timeout");
		}

		/// <summary>
		/// 把代理返回给代理池
		/// </summary>
		/// <param name="proxy">代理</param>
		/// <param name="statusCode">通过此代理请求数据后的返回状态</param>
		public void ReturnProxy(UseSpecifiedUriWebProxy proxy, HttpStatusCode statusCode)
		{
			if (proxy == null)
			{
				return;
			}
			var key = $"{proxy.Uri.Host}:{proxy.Uri.Port}";
			if (!_allProxy.ContainsKey(key))
			{
				return;
			}
			Proxy p = _allProxy[key];
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
			if (p.FailedNum > 20)
			{
				return;
			}
			if (ProxyValidator != null && p.FailedNum % 3 == 0 && ProxyValidator.IsAvailable(proxy))
			{
				return;
			}
			lock (_locker)
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

		private void RefreshProxies()
		{
			if (ProxyValidator != null)
			{
				var result = _supplier.GetProxies();

				Parallel.ForEach(result, new ParallelOptions
				{
					MaxDegreeOfParallelism = 4
				}, proxy =>
				{
					var key = proxy.Key;
					var value = proxy.Value;
					if (_allProxy.ContainsKey(key))
					{
						return;
					}

					if (ProxyValidator.IsAvailable(proxy.Value.WebProxy))
					{
						value.SetFailedNum(0);
						value.SetReuseTime(_reuseInterval);

						lock (this)
						{
							_proxyQueue.Add(value);
						}
						_allProxy.GetOrAdd(key, value);
					}
				});
			}
		}
	}
}