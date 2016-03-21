using System;
using Java2Dotnet.Spider.Redial.RedialManager;

namespace Java2Dotnet.Spider.Redial.AtomicExecutor
{
	public interface IAtomicExecutor
	{
		void Execute(string name, Action action);
		void Execute(string name, Action<object> action, object obj);
		T Execute<T>(string name, Func<object, T> func, object obj);
		T Execute<T>(string name, Func<T> func);
		void WaitAtomicAction();
		IWaitforRedial WaitforRedial { get; }
	}
}
