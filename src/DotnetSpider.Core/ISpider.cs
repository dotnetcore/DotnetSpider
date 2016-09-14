using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public interface ISpider : IDisposable, ITask
	{
		/// <summary>
		/// Site of a task
		/// </summary>
		Site Site { get; }
		Dictionary<string, dynamic> Settings { get; }
		void Run(params string[] arguments);
		Task RunAsync(params string[] arguments);
		void Stop();
		void Exit();
	}
}
