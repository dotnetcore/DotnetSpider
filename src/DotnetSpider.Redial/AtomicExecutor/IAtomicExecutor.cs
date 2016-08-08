using System;
using DotnetSpider.Redial.RedialManager;

namespace DotnetSpider.Redial.AtomicExecutor
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
