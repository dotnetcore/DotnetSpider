using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using System;
using System.Collections.Generic;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public interface ISpider : IDisposable, ITask
	{
		/// <summary>
		/// Unique id for a task.
		/// </summary>
		string Identity { get; }

		/// <summary>
		/// Site of a task
		/// </summary>
		Site Site { get; }

		int ThreadNum { get; }

		Dictionary<string, dynamic> Settings { get; }

		IScheduler Scheduler { get; }

		List<IPipeline> Pipelines { get; }

		IPageProcessor PageProcessor { get; }

		void Run();

		void Stop();

		void Exit();
	}
}
