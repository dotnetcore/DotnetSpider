using System;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public interface ISpider : IDisposable, ITask, IRunable
	{
		Site Site { get; }
	}
}
