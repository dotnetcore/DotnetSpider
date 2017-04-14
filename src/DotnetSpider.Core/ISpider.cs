using DotnetSpider.Core.Downloader;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
