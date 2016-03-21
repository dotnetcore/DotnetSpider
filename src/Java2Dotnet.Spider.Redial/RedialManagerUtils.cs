using Java2Dotnet.Spider.Redial.RedialManager;
using System;

namespace Java2Dotnet.Spider.Redial
{
	public static class RedialManagerUtils
	{
		public static IRedialManager RedialManager;

		public static void Execute(string name, Action action)
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

		public static void Execute(string name, Action<object> action, object obj)
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

		public static T Execute<T>(string name, Func<object, T> func, object obj)
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

		public static T Execute<T>(string name, Func<T> func)
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
