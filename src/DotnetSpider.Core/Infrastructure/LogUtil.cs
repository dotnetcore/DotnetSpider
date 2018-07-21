using DotnetSpider.Common;
using Serilog;
using Serilog.Events;
#if !NET40
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
#endif

namespace DotnetSpider.Core.Infrastructure
{
	public static class LogUtil
	{
		public const string Identity = "System";

#if !NET40
		public static SystemConsoleTheme Spider { get; } = new SystemConsoleTheme(
			new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
			{
				[ConsoleThemeStyle.Text] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
				[ConsoleThemeStyle.SecondaryText] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.DarkGray },
				[ConsoleThemeStyle.TertiaryText] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.DarkGray },
				[ConsoleThemeStyle.Invalid] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
				[ConsoleThemeStyle.Null] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
				[ConsoleThemeStyle.Name] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
				[ConsoleThemeStyle.String] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
				[ConsoleThemeStyle.Number] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
				[ConsoleThemeStyle.Boolean] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
				[ConsoleThemeStyle.Scalar] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
				[ConsoleThemeStyle.LevelVerbose] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Cyan },
				[ConsoleThemeStyle.LevelDebug] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.DarkGray },
				[ConsoleThemeStyle.LevelInformation] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
				[ConsoleThemeStyle.LevelWarning] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
				[ConsoleThemeStyle.LevelError] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Red },
				[ConsoleThemeStyle.LevelFatal] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Red }
			});
#endif

		public static void Init()
		{
			if (Logger.Default is SilentLogger)
			{
				Logger.Default = Create(Identity);
			}
		}

		public static Common.ILogger Create(string identity)
		{
			var loggerConfiguration = new LoggerConfiguration()
				.MinimumLevel.Verbose()
#if !NET40
				.WriteTo.Console(theme: Spider)
#else
				.WriteTo.Console()
#endif
				.WriteTo.RollingFile("dotnetspider.log")
				.Enrich.WithProperty("Identity", identity).Enrich.WithProperty("NodeId", Env.NodeId);
			if (Env.HubService)
			{
				loggerConfiguration = loggerConfiguration.WriteTo.Http(Env.HubServiceLogUrl, Env.HubServiceToken, LogEventLevel.Verbose, 1);
			}
			return new Serilogger(loggerConfiguration.CreateLogger());
		}

		public static Common.ILogger CreateFailingRequestsLogger(string identity)
		{
			var loggerConfiguration = new LoggerConfiguration()
							.MinimumLevel.Verbose()
				.WriteTo.RollingFile($"{identity}.failing.log");
			return new Serilogger(loggerConfiguration.CreateLogger());
		}
	}
}
