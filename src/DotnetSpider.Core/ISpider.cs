using System;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public interface ISpider : IDisposable, IControllable, IIdentity, INamed, ITask
	{
		Site Site { get; }
	}
}
