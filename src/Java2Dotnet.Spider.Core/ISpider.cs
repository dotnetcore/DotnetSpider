using System;
using System.Collections.Generic;

namespace Java2Dotnet.Spider.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public interface ISpider : IDisposable
	{
		/// <summary>
		/// Unique id for a task.
		/// </summary>
		string Identity { get; }

		string UserId { get; }

		string TaskGroup { get; }

		/// <summary>
		/// Site of a task
		/// </summary>
		Site Site { get; }

		void Run();

		void Stop();

		Dictionary<string, dynamic> Settings { get; }
	}
}
