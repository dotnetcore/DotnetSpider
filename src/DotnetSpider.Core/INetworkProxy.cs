using System;

namespace DotnetSpider.Core
{
	public interface INetworkProxy
	{
		void Execute(string name, Action action);
		void Execute(string name, Action<object> action, object obj);
		T Execute<T>(string name, Func<object, T> func, object obj);
		T Execute<T>(string name, Func<T> func);
	}

	public interface INetworkProxyManager : INetworkProxy
	{
		void Register(INetworkProxy proxy);
		INetworkProxy Proxy { get; }
	}

	public class NetworkProxyManager : INetworkProxyManager
	{
		private static INetworkProxyManager instance;
		public INetworkProxy Proxy { get; set; }

		public static INetworkProxyManager Current
		{
			get
			{
				if (instance == null)
				{
					instance = new NetworkProxyManager();
				}
				return instance;
			}
		}

		public void Register(INetworkProxy proxy)
		{
			this.Proxy = proxy;
		}

		public void Execute(string name, Action action)
		{
			if (Proxy != null)
			{
				Proxy.Execute(name, action);
			}
			else
			{
				action();
			}
		}

		public void Execute(string name, Action<object> action, object obj)
		{
			if (Proxy != null)
			{
				Proxy.Execute(name, action, obj);
			}
			else
			{
				action(obj);
			}
		}

		public T Execute<T>(string name, Func<T> func)
		{
			if (Proxy != null)
			{
				return Proxy.Execute(name, func);
			}
			else
			{
				return func();
			}
		}

		public T Execute<T>(string name, Func<object, T> func, object obj)
		{
			if (Proxy != null)
			{
				return Proxy.Execute(name, func, obj);
			}
			else
			{
				return func(obj);
			}
		}
	}
}
