using DotnetSpider.Core;
using DotnetSpider.Redial.RedialManager;
using System;

namespace DotnetSpider.Redial
{
	public class RedialExecutor : INetworkProxy
	{
		public IRedialManager RedialManager;

		public RedialExecutor(IRedialManager redialManager)
		{
			if (redialManager == null)
			{
				throw new SpiderException("RedialManager should not be null.");
			}
			RedialManager = redialManager;
		}

		public void Redial()
		{
			RedialManager.Redial();
		}

		public void Execute(string name, Action action)
		{
			if (RedialManager != null)
			{
				RedialManager.AtomicExecutor.Execute(name, action);
			}
			else
			{
				action();
			}
		}

		public void Execute(string name, Action<object> action, object obj)
		{
			if (RedialManager != null)
			{
				RedialManager.AtomicExecutor.Execute(name, action, obj);
			}
			else
			{
				action(obj);
			}
		}

		public T Execute<T>(string name, Func<object, T> func, object obj)
		{
			if (RedialManager != null)
			{
				return RedialManager.AtomicExecutor.Execute(name, func, obj);
			}
			else
			{
				return func(obj);
			}
		}

		public T Execute<T>(string name, Func<T> func)
		{
			if (RedialManager != null)
			{
				return RedialManager.AtomicExecutor.Execute(name, func);
			}
			else
			{
				return func();
			}
		}
	}
}
