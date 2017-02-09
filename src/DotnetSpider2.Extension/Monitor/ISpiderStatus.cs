using System;
namespace DotnetSpider.Extension.Monitor
{
	public interface ISpiderStatus
	{
		string Name { get; }

		string Status { get; }

		int ThreadCount { get; }

		long TotalPageCount { get; }

		long LeftPageCount { get; }

		long SuccessPageCount { get; }

		long ErrorPageCount { get; }

		DateTime StartTime { get; }

		double Speed { get; }
	}
}
