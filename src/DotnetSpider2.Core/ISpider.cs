using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public interface ISpider : IDisposable, ITask, IRunable, IStopable, IExitable
	{
		/// <summary>
		/// Site of a task
		/// </summary>
		Site Site { get; }
		Task RunAsync(params string[] arguments);
	}
}
