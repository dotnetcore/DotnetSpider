using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSpider.Core.Infrastructure
{
	public interface INetworkExecutor
	{
		void Execute(string name, Action action);
		void Execute(string name, Action<object> action, object obj);
		T Execute<T>(string name, Func<object, T> func, object obj);
		T Execute<T>(string name, Func<T> func);
	}
}
