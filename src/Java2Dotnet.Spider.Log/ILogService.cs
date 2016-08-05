using Java2Dotnet.Spider.Ioc;
using System;

namespace Java2Dotnet.Spider.Log
{
	public interface ILogService : IService, IDisposable
	{
		void Warn(string message, Exception e);
		void Warn(string message);
		void Info(string message, Exception e);
		void Info(string message);
		void Error(string message, Exception e);
		void Error(string message);
	}
}
