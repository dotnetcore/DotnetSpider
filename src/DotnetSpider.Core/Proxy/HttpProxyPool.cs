using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Core.Proxy
{
	public class HttpProxyPool : IHttpProxyPool
	{
		private readonly IProxySupplier _supplier;
		private readonly List<Proxy> _proxyQueue = new List<Proxy>();
		private readonly ConcurrentDictionary<string, Proxy> _allProxy = new ConcurrentDictionary<string, Proxy>();
		private bool _isDispose;
		private readonly int _reuseInterval;

		public HttpProxyPool(IProxySupplier supplier, int reuseInterval = 500)
		{
			_supplier = supplier ?? throw new SpiderException("IProxySupplier should not be null.");

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

		public UseSpecifiedUriWebProxy GetProxy()
		{
			for (int i = 0; i < 3600; ++i)
			{
				lock (this)
				{
					var proxy = _proxyQueue.FirstOrDefault(p => DateTimeUtils.GetCurrentTimeStamp() - p.GetLastUseTime() > _reuseInterval);
					if (proxy != null)
					{
						proxy.SetLastBorrowTime(DateTimeUtils.GetCurrentTimeStamp());
						_proxyQueue.Remove(proxy);
						return proxy.GetWebProxy();
					}
				}
				Thread.Sleep(1000);
			}

			throw new SpiderException("Get proxy timeout.");
		}

		public void ReturnProxy(UseSpecifiedUriWebProxy host, HttpStatusCode statusCode)
		{
			if (host == null)
			{
				return;
			}
			var key = $"{host.Uri.Host}:{host.Uri.Port}";
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
			if (p.FailedNum % 3 == 0 && !ProxyUtil.ValidateProxy(p.HttpHost.Uri.Host, p.HttpHost.Uri.Port))
			{
				return;
			}
			lock (this)
			{
				_proxyQueue.Add(p);
			}
		}

		private void RefreshProxies()
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

				if (ProxyUtil.ValidateProxy(proxy.Value.HttpHost.Uri.Host, proxy.Value.HttpHost.Uri.Port))
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

		public void Dispose()
		{
			_isDispose = true;
		}
	}
}