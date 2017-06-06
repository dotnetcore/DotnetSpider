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

		private readonly int _reuseInterval;

		public HttpProxyPool(IProxySupplier supplier, int reuseInterval = 500)
		{
			_supplier = supplier ?? throw new SpiderException("IProxySupplier should not be null.");

			_reuseInterval = reuseInterval;

			Task.Factory.StartNew(() =>
			{
				for (long i = 0; i < long.MaxValue; i++)
				{
					if (_proxyQueue.Count < 50)
					{
						UpdateProxy();
					}
					Thread.Sleep(30000);
				}
			});
		}

		private void UpdateProxy()
		{
			var result = _supplier.GetProxies();

			Parallel.ForEach(result, new ParallelOptions
			{
				MaxDegreeOfParallelism = 10
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
					LogCenter.Log(null, $"Detect one usefull proxy: {key}", LogLevel.Debug);
					value.SetFailedNum(0);
					value.SetReuseTime(_reuseInterval);

					_proxyQueue.Add(value);
					_allProxy.GetOrAdd(key, value);
				}
			});
		}

		public UseSpecifiedUriWebProxy GetProxy()
		{
			for (int i = 0; i < 3600; ++i)
			{
				var proxy = _proxyQueue.FirstOrDefault(p => DateTimeUtils.GetCurrentTimeStamp() - p.GetLastUseTime() > _reuseInterval);
				if (proxy != null)
				{
					proxy.SetLastBorrowTime(DateTimeUtils.GetCurrentTimeStamp());
					_proxyQueue.Remove(proxy);
					return proxy.GetWebProxy();
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

			_proxyQueue.Add(p);
		}
	}
}