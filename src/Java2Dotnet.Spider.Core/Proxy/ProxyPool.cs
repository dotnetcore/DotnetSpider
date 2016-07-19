using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Java2Dotnet.Spider.Common;
using Java2Dotnet.Spider.Log;
using Newtonsoft.Json;

namespace Java2Dotnet.Spider.Core.Proxy
{
	public class ProxyPool
	{
		private readonly ConcurrentQueue<Proxy> _proxyQueue = new ConcurrentQueue<Proxy>();
		private readonly ConcurrentDictionary<string, Proxy> _allProxy = new ConcurrentDictionary<string, Proxy>();

		private int _reuseInterval = 1500;// ms
		private int _reviveTime = 2 * 60 * 60 * 1000;// ms

		private bool _validateWhenInit = false;
		private string _proxyFile = "data/lastUse.proxy";

		public ProxyPool()
		{
		}

		public ProxyPool(List<string[]> httpProxyList)
		{
			ReadProxyList();
			IDictionary<string, Proxy> tmp = new Dictionary<string, Proxy>();
			foreach (var proxy in httpProxyList)
			{
				tmp.Add(proxy[0], new Proxy(new HttpHost(proxy[0], int.Parse(proxy[1]))));
			}
			AddProxy((IDictionary)tmp);
			// ReSharper disable once ObjectCreationAsStatement
			new Timer(o =>
			{
				SaveProxyList();
				//Logger.Info(AllProxyStatus());
			}, null, 10 * 60 * 1000, (10 * 60 * 1000));
		}

		private void SaveProxyList()
		{
			if (_allProxy.Count == 0)
			{
				return;
			}

			try
			{
				File.WriteAllText(_proxyFile, JsonConvert.SerializeObject(_allProxy));
				//Logger.Info("Save proxy");
			}
			catch (FileNotFoundException e)
			{
				//Logger.Error("Proxy file not found", e);
			}
			catch (IOException e)
			{
				//Logger.Error(e.ToString());
			}
		}

		private void ReadProxyList()
		{
			try
			{
				string json = File.ReadAllText(_proxyFile);
				IDictionary data = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Proxy>>(json);
				AddProxy(data);
			}
			catch (FileNotFoundException e)
			{
				//Logger.Error("Proxy file not found", e);
			}
			catch (IOException e)
			{
				//Logger.Error(e.ToString());
			}
		}

		private void AddProxy(IDictionary httpProxyMap)
		{
			Enable = true;
			var enumerator = httpProxyMap.GetEnumerator();
			while (enumerator.MoveNext())
			{
				try
				{
					var key = enumerator.Key.ToString();
					var value = (Proxy)enumerator.Value;
					if (_allProxy.ContainsKey(key))
					{
						continue;
					}
					if (!_validateWhenInit || ProxyUtil.ValidateProxy(value.GetHttpHost()))
					{
						value.SetFailedNum(0);
						value.SetReuseTimeInterval(_reuseInterval);
						_proxyQueue.Enqueue(value);
						_allProxy.GetOrAdd(key, value);
					}
				}
				catch (Exception e)
				{
					//Logger.Error("HttpHost init error:", e);
				}
			}
			//Logger.Info("proxy pool size>>>>" + _allProxy.Count);
		}

		public HttpHost GetProxy()
		{
			Proxy proxy = null;
			try
			{
				double time = DateTimeUtils.GetCurrentTimeStamp();

				if (_proxyQueue.TryDequeue(out proxy))
				{
					double costTime = (DateTimeUtils.GetCurrentTimeStamp() - time) / 1000.0;
					if (costTime > _reuseInterval)
					{
						//Logger.Info("get proxy time >>>> " + costTime);
					}
					Proxy p;
					if (_allProxy.TryGetValue(proxy.GetHttpHost().Host, out p))
					{
						p.SetLastBorrowTime(DateTimeUtils.GetCurrentTimeStamp());
						p.BorrowNumIncrement(1);
					}
				}
			}
			catch (Exception e)
			{
				//Logger.Error("Get proxy error", e);
			}
			if (proxy == null)
			{
				return null;
			}
			return proxy.GetHttpHost();
		}

		public void ReturnProxy(HttpHost host, int statusCode)
		{
			Proxy p = _allProxy[host.Host];
			if (p == null)
			{
				return;
			}
			switch (statusCode)
			{
				case Proxy.Success:
					p.SetReuseTimeInterval(_reuseInterval);
					p.SetFailedNum(0);
					p.SetFailedErrorType(new List<int>());
					p.RecordResponse();
					p.SuccessNumIncrement(1);
					break;
				case Proxy.Error403:
					p.Fail(Proxy.Error403);
					p.SetReuseTimeInterval(_reuseInterval * p.FailedNum);
					//Logger.Info(host + " >>>> reuseTimeInterval is >>>> " + p.GetReuseTimeInterval() / 1000.0);
					break;
				case Proxy.ErrorBanned:
					p.Fail(Proxy.ErrorBanned);
					p.SetReuseTimeInterval(10 * 60 * 1000 * p.FailedNum);
					//Logger.Warn("this proxy is banned >>>> " + p.GetHttpHost());
					//Logger.Info(host + " >>>> reuseTimeInterval is >>>> " + p.GetReuseTimeInterval() / 1000.0);
					break;
				case Proxy.Error404:
					p.Fail(Proxy.Error404);
					p.SetReuseTimeInterval(_reuseInterval * p.FailedNum);
					break;
				default:
					p.Fail(statusCode);
					break;
			}
			if (p.FailedNum > 20)
			{
				// allProxy.remove(host.getAddress().getHostAddress());
				p.SetReuseTimeInterval(_reviveTime);
				//Logger.Error($"Remove proxy {host}: p.GetFailedType(). Remain proxy: { _proxyQueue.Count}");
				return;
			}
			if (p.FailedNum % 5 == 0)
			{
				if (!ProxyUtil.ValidateProxy(host))
				{
					// allProxy.remove(host.getAddress().getHostAddress());
					p.SetReuseTimeInterval(_reviveTime);
					//Logger.Error("remove proxy >>>> " + host + ">>>>" + p.GetFailedType() + " >>>> remain proxy >>>> " + _proxyQueue.Count);
					return;
				}
			}
			try
			{
				_proxyQueue.Enqueue(p);
			}
			catch (Exception e)
			{
				//Logger.Warn("proxyQueue return proxy error", e);
			}
		}

		public string AllProxyStatus()
		{
			return _allProxy.Aggregate("all proxy info >>>> \n", (current, entry) => current + (entry.Value + "\n"));
		}

		public int GetIdleNum()
		{
			return _proxyQueue.Count;
		}

		public int GetReuseInterval()
		{
			return _reuseInterval;
		}

		public void SetReuseInterval(int reuseInterval)
		{
			_reuseInterval = reuseInterval;
		}

		public static List<string[]> GetProxyList()
		{
			List<string[]> proxyList = new List<string[]>();

			try
			{
				using (StreamReader reader = new StreamReader(File.OpenRead("proxy.txt")))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						var datas = line.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
						proxyList.Add(new[] { datas[0], datas[1] });
					}
				}
			}
			catch (FileNotFoundException e)
			{
				//Logger.Error(e.ToString());
			}
			catch (IOException e)
			{
				//Logger.Error(e.ToString());
			}
			return proxyList;
		}

		public static void Main(string[] args)
		{
			ProxyPool proxyPool = new ProxyPool(GetProxyList());
			proxyPool.SetReuseInterval(10000);
			proxyPool.SaveProxyList();

			while (true)
			{
				List<HttpHost> httphostList = new List<HttpHost>();
				Console.Read();
				int i = 0;
				while (proxyPool.GetIdleNum() > 2)
				{
					HttpHost httphost = proxyPool.GetProxy();
					httphostList.Add(httphost);
					// proxyPool.proxyPool.use(httphost);
					//Logger.Info("borrow object>>>>" + i + ">>>>" + httphostList[i]);
					i++;
				}
				Console.WriteLine(proxyPool.AllProxyStatus());
				Console.Read();
				for (i = 0; i < httphostList.Count; i++)
				{
					proxyPool.ReturnProxy(httphostList[i], 200);
					//Logger.Info("return object>>>>" + i + ">>>>" + httphostList[i]);
				}
				Console.WriteLine(proxyPool.AllProxyStatus());
				Console.Read();
			}
			// ReSharper disable once FunctionNeverReturns
		}


		public bool Enable { get; set; }
	}
}