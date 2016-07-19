using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Log
{
	public class FileLog : ILogService
	{
		private static readonly string LogFile;
		private static StreamWriter Writter;

		static FileLog()
		{
#if NET_CORE
			LogFile = Path.Combine(AppContext.BaseDirectory, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
#else
			LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
#endif
			int i = 1;
			while (true)
			{
				try
				{
					Writter = File.AppendText(LogFile);
					break;
				}
				catch
				{
					LogFile += $".{i}";
				}
			}
		}

		public void Error(dynamic message)
		{
			WriteToFileLog("[错误] " + message);
		}

		public void Error(dynamic message, Exception e)
		{
			WriteToFileLog("[错误] " + message + ": " + e);
		}

		public void Info(dynamic message)
		{
			WriteToFileLog("[信息] " + message);
		}

		public void Info(dynamic message, Exception e)
		{
			WriteToFileLog("[信息] " + message + ": " + e);
		}

		public void Warn(dynamic message)
		{
			WriteToFileLog("[警告] " + message);
		}

		public void Warn(dynamic message, Exception e)
		{
			WriteToFileLog("[警告]" + message + ": " + e);
		}

		private static void WriteToFileLog(string log)
		{
			lock (LogFile)
			{
				Writter.WriteLine(log.ToString());
				Writter.Flush();
			}
		}

		public void Dispose()
		{
		}
	}
}
