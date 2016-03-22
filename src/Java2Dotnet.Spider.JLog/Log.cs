using System;
using System.IO;
using System.Text;

namespace Java2Dotnet.Spider.JLog
{
	public class Log : ILog
	{
		private static readonly object WriteToConsoleLocker = new object();
		private static readonly object WriteToLogFileLocker = new object();
		private static readonly string LogFile;

		public string Name { get; }

		static Log()
		{
			LogFile = Path.Combine(Directory.GetCurrentDirectory(), DateTime.Now.ToString("yyyy-MM-dd") + ".log");
		}

		public Log(string name)
		{
			Name = name;
		}

		public void Warn(string message, Exception e, bool showToConsole)
		{
			// TODO: save to database
			string time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
			string msg = $"[WARNING] {time} {message + Environment.NewLine} {e}";
			WriteToLogFile(msg);
			if (showToConsole)
			{
				WriteToConsole(msg, Level.Warning);
			}
		}

		public void Warn(string message, bool showToConsole)
		{
			Warn(message, null, showToConsole);
		}

		public void Info(string message, Exception e, bool showToConsole)
		{
			// TODO: save to database
			string time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
			string msg = $"[INFO] {time} {message + Environment.NewLine} {e}";
			WriteToLogFile(msg);
			if (showToConsole)
			{
				WriteToConsole(msg, Level.Info);
			}
		}

		public void Info(string message, bool showToConsole)
		{
			Info(message, null, showToConsole);
		}

		public void Error(string message, Exception e, bool showToConsole)
		{
			// TODO: save to database
			string time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
			string msg = $"[ERROR] {time} {message + Environment.NewLine} {e}";
			WriteToLogFile(msg);
			if (showToConsole)
			{
				WriteToConsole(msg, Level.Error);
			}
		}

		public void Error(string message, bool showToConsole)
		{
			Error(message, null, showToConsole);
		}

		private static void WriteToConsole(string message, Level level)
		{
			lock (WriteToConsoleLocker)
			{
				switch (level)
				{
					case Level.Error:
						{
							Console.ForegroundColor = ConsoleColor.Red;
							break;
						}
					case Level.Info:
						{
                            Console.ForegroundColor = ConsoleColor.Magenta;
							break;
						}
					case Level.Warning:
						{
							Console.ForegroundColor = ConsoleColor.Yellow;
							break;
						}
				}
				Console.WriteLine(message);
				Console.ForegroundColor = ConsoleColor.White;
			}
		}

		private static void WriteToLogFile(string message)
		{
			lock (WriteToLogFileLocker)
			{
				File.AppendAllText(LogFile, message,Encoding.UTF8);
			}
		}
	}
}
