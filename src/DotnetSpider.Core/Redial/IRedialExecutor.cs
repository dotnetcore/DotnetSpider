using System;

namespace DotnetSpider.Core.Redial
{
	public interface IRedialExecutor
	{
		RedialResult Redial(Action action = null);
		void WaitAll();
		string CreateActionIdentity(string name);
		void DeleteActionIdentity(string identity);
		void Execute(string name, Action action);
		void Execute(string name, Action<dynamic> action, dynamic obj);
		T Execute<T>(string name, Func<dynamic, T> func, dynamic obj);
		T Execute<T>(string name, Func<T> func);
	}
}
