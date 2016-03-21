using System;

namespace Java2Dotnet.Spider.JLog
{
	public interface ILog
	{
		void Warn(string message, Exception e, bool showToConsole = true);
		void Warn(string message, bool showToConsole = true);
		void Info(string message, Exception e, bool showToConsole = true);
		void Info(string message, bool showToConsole = true);

		void Error(string message, Exception e, bool showToConsole = true);
		void Error(string message, bool showToConsole = true);

		string Name { get; }
	}
}
