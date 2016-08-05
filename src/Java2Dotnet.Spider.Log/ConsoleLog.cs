using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Java2Dotnet.Spider.Log
{
	public class ConsoleLog : ILogService
	{
		private static readonly object WriteToConsoleLocker = new object();

		public void Error(string message)
		{
			WriteToConsole("错误", message.ToString());
		}

		public void Error(string message, Exception e)
		{
			WriteToConsole("错误", message + ": " + e);
		}

		public void Info(string message)
		{
			WriteToConsole("信息", message.ToString());
		}

		public void Info(string message, Exception e)
		{
			WriteToConsole("信息", message + ": " + e);
		}

		public void Warn(string message)
		{
			WriteToConsole("警告", message.ToString());
		}

		public void Warn(string message, Exception e)
		{
			WriteToConsole("警告", message + ": " + e);
		}

		private static void WriteToConsole(string type, string log)
		{
			lock (WriteToConsoleLocker)
			{
				switch (type)
				{
					case "错误":
						{
							Console.ForegroundColor = ConsoleColor.Red;
							break;
						}
					case "信息":
						{
							Console.ForegroundColor = ConsoleColor.White;
							break;
						}
					case "警告":
						{
							Console.ForegroundColor = ConsoleColor.Yellow;
							break;
						}
				}

				Console.WriteLine(log);
				Console.ForegroundColor = ConsoleColor.White;
			}
		}

		public void Dispose()
		{
		}
	}
}
