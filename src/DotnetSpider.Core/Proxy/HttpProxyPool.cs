using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Core.Common;
using NLog;

namespace DotnetSpider.Core.Proxy
{
	public class HttpProxyPool
	{
		private readonly IProxySupplier _supplier;
		private readonly List<Proxy> _proxyQueue = new List<Proxy>();
		private readonly ConcurrentDictionary<string, Proxy> _allProxy = new ConcurrentDictionary<string, Proxy>();

		private ILogger Logger { get; }
		private readonly int _reuseInterval;

		public HttpProxyPool(IProxySupplier supplier, int reuseInterval = 1500)
		{
			if (supplier == null)
			{
				throw new SpiderException("IProxySupplier should not be null.");
			}
			_supplier = supplier;
			Logger = LogManager.GetCurrentClassLogger();

			_reuseInterval = reuseInterval;

			var timer = new Timer((a) =>
			{
				if (_proxyQueue.Count < 50)
				{
					UpdateProxy();
				}
			}, null, 1, 30000);
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
					Logger.Debug($"Detect one usefull proxy: {key}");
					value.SetFailedNum(0);
					value.SetReuseTime(_reuseInterval);

					_proxyQueue.Add(value);
					_allProxy.GetOrAdd(key, value);
				}
			});
		}

		public UseSpecifiedUriWebProxy GetProxy()
		{
			for (int i = 0; i < 60; ++i)
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

		public int GetIdleNum()
		{
			return _proxyQueue.Count;
		}
	}
}