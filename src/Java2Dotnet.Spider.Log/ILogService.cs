using Java2Dotnet.Spider.Ioc;
using System;

namespace Java2Dotnet.Spider.Log
{
	public interface ILogService : IService,IDisposable
	{
		void Warn(dynamic message, Exception e);
		void Warn(dynamic message);
		void Info(dynamic message, Exception e);
		void Info(dynamic message);
		void Error(dynamic message, Exception e);
		void Error(dynamic message);
	}
}
