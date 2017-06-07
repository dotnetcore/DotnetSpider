using System;

namespace DotnetSpider.Core.Infrastructure
{
	public enum LogLevel
	{
		Trace,
		Debug,
		Info,
		Warn,
		Error,
		Fatal,
		Off
	}

	public interface ILogger
	{
		void Log(IIdentity spider, string node, string message, LogLevel level, Exception e = null);
	}
}
