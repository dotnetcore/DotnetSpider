using System;

namespace DotnetSpider.Core
{
	public interface INetworkExecutor
	{
		void Execute(string name, Action action);
		void Execute(string name, Action<object> action, object obj);
		T Execute<T>(string name, Func<object, T> func, object obj);
		T Execute<T>(string name, Func<T> func);
	}

	public class NetworkCenter
	{
		private static NetworkCenter _instance;
		public INetworkExecutor Executor { get; set; }

		public static NetworkCenter Current => _instance ?? (_instance = new NetworkCenter());

		public void Register(INetworkExecutor executor)
		{
			Executor = executor;
		}

		public void Execute(string name, Action action)
		{
			if (Executor != null)
			{
				Executor.Execute(name, action);
			}
			else
			{
				action();
			}
		}

		public void Execute(string name, Action<object> action, object obj)
		{
			if (Executor != null)
			{
				Executor.Execute(name, action, obj);
			}
			else
			{
				action(obj);
			}
		}

		public T Execute<T>(string name, Func<T> func)
		{
			if (Executor != null)
			{
				return Executor.Execute(name, func);
			}
			else
			{
				return func();
			}
		}

		public T Execute<T>(string name, Func<object, T> func, object obj)
		{
			if (Executor != null)
			{
				return Executor.Execute(name, func, obj);
			}
			else
			{
				return func(obj);
			}
		}
	}
}
