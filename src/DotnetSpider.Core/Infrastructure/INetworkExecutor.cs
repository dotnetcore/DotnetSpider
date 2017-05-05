using System;

namespace DotnetSpider.Core.Infrastructure
{
	public interface INetworkExecutor
	{
		void Execute(string name, Action action);
		void Execute(string name, Action<dynamic> action, dynamic obj);
		T Execute<T>(string name, Func<dynamic, T> func, dynamic obj);
		T Execute<T>(string name, Func<T> func);
	}
}
