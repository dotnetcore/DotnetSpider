using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace Java2Dotnet.Spider.JLog
{
	internal class LogInfo
	{
		public string Type { get; set; }
		public string Time { get; set; }
		public string Message { get; set; } = "";
		public string Machine = Log.Machine;
		public string TaskGroup { get; set; } = "";
		public string UserId { get; set; } = "ooodata";

		public override string ToString()
		{
			return $"[{Type}] {Time} [{Machine}][{UserId}][{TaskGroup}] {Message}";
		}
	}

	public class Log : ILog
	{
		private static readonly object WriteToConsoleLocker = new object();
		private static readonly object WriteToLogFileLocker = new object();
		private static readonly string LogFile;
		public static string Machine;
		public static bool NoConsole = false;
		public string Name { get; }
		public static string LogServer;
		private static SynchronizedList<Task<HttpResponseMessage>> LogUpLoadTasks = new SynchronizedList<Task<HttpResponseMessage>>();
		private static StreamWriter Writter;
		private string UserId { get; }
		private string TaskGroup { get; }
		private bool _saveToLogService = false;
		static Log()
		{
			Machine = Dns.GetHostName();
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

			LogServer = ConfigurationManager.Get("logHost");
			var noConsoleProperty = ConfigurationManager.Get("noConsoleLog");
			if (!string.IsNullOrEmpty(noConsoleProperty))
			{
				NoConsole = bool.Parse(noConsoleProperty);
			}
		}

		public static void WaitForExit()
		{
			while (true)
			{
				if (LogUpLoadTasks.Count() == 0)
				{
					break;
				}
				Thread.Sleep(500);
			}
		}

		public Log(string name)
		{
			Name = name;

			if (name.Contains("&"))
			{
				//"dotnetspider&dotnetspider&task1 weekly"
				var arguments = name.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
				UserId = arguments[1];
				TaskGroup = arguments[2];

				if (!string.IsNullOrEmpty(LogServer))
				{
					_saveToLogService = true;
				}
			}
		}

		public void Warn(string message, Exception e, bool showToConsole = true)
		{
			var log = CreateLogInfo("WARNING", message, e);
			WriteToLogFile(log);
			if (showToConsole)
			{
				WriteToConsole(log);
			}
			if (_saveToLogService)
			{
				WriteToLogService(log);
			}
		}

		public void Warn(string message, bool showToConsole = true)
		{
			Warn(message, null, showToConsole);
		}

		public void Info(string message, Exception e, bool showToConsole = true)
		{
			var log = CreateLogInfo("INFO", message, e);
			WriteToLogFile(log);
			if (showToConsole)
			{
				WriteToConsole(log);
			}
			if (_saveToLogService)
			{
				WriteToLogService(log);
			}
		}

		public void Info(string message, bool showToConsole = true)
		{
			Info(message, null, showToConsole);
		}

		public void Error(string message, Exception e, bool showToConsole = true)
		{
			var log = CreateLogInfo("ERROR", message, e);
			WriteToLogFile(log);
			if (showToConsole)
			{
				WriteToConsole(log);
			}
			if (_saveToLogService)
			{
				WriteToLogService(log);
			}
		}

		public void Error(string message, bool showToConsole = true)
		{
			Error(message, null, showToConsole);
		}

		public static void WriteLine(string message)
		{
			if (NoConsole)
			{
				return;
			}

			lock (WriteToConsoleLocker)
			{
				try
				{
					Console.WriteLine(message);
				}
				catch
				{
				}
			}
		}

		private static void WriteToConsole(LogInfo log)
		{
			if (NoConsole)
			{
				return;
			}

			lock (WriteToConsoleLocker)
			{
				switch (log.Type)
				{
					case "ERROR":
						{
							Console.ForegroundColor = ConsoleColor.Red;
							break;
						}
					case "INFO":
						{
							Console.ForegroundColor = ConsoleColor.White;
							break;
						}
					case "WARNING":
						{
							Console.ForegroundColor = ConsoleColor.Yellow;
							break;
						}
				}

				WriteLine(log.ToString());
				Console.ForegroundColor = ConsoleColor.White;
			}
		}

		private LogInfo CreateLogInfo(string type, string message, Exception e)
		{
			string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			var log = new LogInfo()
			{
				Type = type,
				Time = time,
				TaskGroup = TaskGroup,
				UserId = UserId,
				Message = message + Environment.NewLine + e
			};
			return log;
		}

		private static void WriteToLogFile(LogInfo log)
		{
			lock (WriteToLogFileLocker)
			{
				Writter.WriteLine(log.ToString());
				Writter.Flush();
			}
		}

		private static void WriteToLogService(LogInfo log)
		{
			HttpClient client = new HttpClient();
			StringBuilder builder = new StringBuilder("{ \"Type\": \"");
			builder.Append(log.Type).Append("\", \"Time\": \"").Append(log.Time).Append("\", \"Message\": \"").Append(log.Message);
			builder.Append("\", \"Machine\": \"").Append(log.Machine);
			builder.Append("\", \"UserId\": \"").Append(string.IsNullOrEmpty(log.UserId) ? "DotnetSpider" : log.UserId);
			builder.Append("\", \"TaskGroup\": \"").Append(string.IsNullOrEmpty(log.TaskGroup) ? "UNKONW" : log.TaskGroup);
			builder.Append("\" }");

			var task = client.PostAsync(LogServer, new StringContent(builder.ToString().Replace("\n", "\\n").Replace("\t", "\\t").Replace("\r", "\\r"), Encoding.UTF8, "application/json"));
			LogUpLoadTasks.Add(task);
			task.ContinueWith((t) =>
			{
				LogUpLoadTasks.Remove(t);
			});
		}
	}
}
