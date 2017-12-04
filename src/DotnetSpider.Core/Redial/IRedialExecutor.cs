using System;

namespace DotnetSpider.Core.Redial
{
    /// <summary>
    /// 重复执行接口
    /// </summary>
	public interface IRedialExecutor : IDisposable
	{
		RedialResult Redial(Action action = null);
		void Execute(string name, Action action);
		void Execute(string name, Action<dynamic> action, dynamic obj);
		T Execute<T>(string name, Func<dynamic, T> func, dynamic obj);
		T Execute<T>(string name, Func<T> func);
	}
}
