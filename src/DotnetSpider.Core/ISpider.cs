using System;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public interface ISpider : IDisposable, IRunable, IIdentity
	{
		Site Site { get; }
	}
}
