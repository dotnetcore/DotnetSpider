using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotnetSpider.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace DotnetSpider
{
	/// <summary>
	/// 启动任务工具
	/// </summary>
	public abstract class Startup
	{
		public static void Execute<TSpider>()
		{
			var logfile = Environment.GetEnvironmentVariable("DOTNET_SPIDER_ID");
			logfile = string.IsNullOrWhiteSpace(logfile) ? "dotnet-spider.log" : $"/logs/{logfile}.log";
			Environment.SetEnvironmentVariable("logfile", logfile);

			if (Log.Logger == null)
			{
				var configure = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Verbose()
#else
					.MinimumLevel.Information()
#endif
					.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
					.Enrich.FromLogContext()
					.WriteTo.Console().WriteTo
					.RollingFile(logfile);

				Log.Logger = configure.CreateLogger();
			}

			Framework.SetEncoding();

			Framework.SetMultiThread();

			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
			configurationBuilder.AddEnvironmentVariables(prefix: "DOTNET_SPIDER_");
			configurationBuilder.AddCommandLine(Environment.GetCommandLineArgs(), Framework.SwitchMappings);
			var configuration = configurationBuilder.Build();
			
			var id = configuration["ID"] ?? Guid.NewGuid().ToString("N");
			var name =  configuration["NAME"]??id;
			var arguments = Environment.GetCommandLineArgs();

			PrintEnvironment();

			var builder = new SpiderHostBuilder();
			builder.ConfigureLogging(b =>
			{
#if DEBUG
				b.SetMinimumLevel(LogLevel.Debug);
#else
				b.SetMinimumLevel(LogLevel.Information);
#endif
				b.AddSerilog();
			});

			var config = configuration["config"];
			builder.ConfigureAppConfiguration(x =>
			{
				if (!string.IsNullOrWhiteSpace(config) && File.Exists(config))
				{
					// 添加 JSON 配置文件
					x.AddJsonFile(config);
				}
				else
				{
					if (File.Exists("appsettings.json"))
					{
						x.AddJsonFile("appsettings.json");
					}
				}

				x.AddEnvironmentVariables(prefix: "DOTNET_SPIDER_");
				x.AddCommandLine(Environment.GetCommandLineArgs(), Framework.SwitchMappings);
			});

			builder.ConfigureServices(services =>
			{
				services.AddLocalEventBus();
				services.AddLocalDownloadCenter();
				services.AddDownloaderAgent(x =>
				{
					x.UseFileLocker();
					x.UseDefaultAdslRedialer();
					x.UseDefaultInternetDetector();
				});
				services.AddStatisticsCenter(x =>
				{
					// 添加内存统计服务
					x.UseMemory();
				});
			});

			var spiderType = typeof(TSpider);
			builder.Register(spiderType);
			var provider = builder.Build();
			var instance = provider.Create(spiderType);
			if (instance != null)
			{
				instance.Name = name;
				instance.Id = id;
				instance.RunAsync(arguments).ConfigureAwait(true).GetAwaiter().GetResult();
			}
			else
			{
				Log.Logger.Error("创建爬虫对象失败", 0, ConsoleColor.DarkYellow);
			}	
		}
		
		/// <summary>
		/// DLL 名字中包含任意一个即是需要扫描的 DLL
		/// </summary>
		protected List<string> DetectAssembles { get; set; } = new List<string> {"spiders"};

		protected abstract void ConfigureService(IConfiguration configuration, SpiderHostBuilder builder);

		/// <summary>
		/// 运行
		/// </summary>
		/// <param name="args">运行参数</param>
		public void Execute(params string[] args)
		{
			try
			{
				var logfile = Environment.GetEnvironmentVariable("DOTNET_SPIDER_ID");
				logfile = string.IsNullOrWhiteSpace(logfile) ? "dotnet-spider.log" : $"/logs/{logfile}.log";
				Environment.SetEnvironmentVariable("logfile", logfile);

				ConfigureSerialLog(logfile);

				Framework.SetEncoding();

				Framework.SetMultiThread();

				var configurationBuilder = new ConfigurationBuilder();
				configurationBuilder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
				configurationBuilder.AddEnvironmentVariables(prefix: "DOTNET_SPIDER_");
				configurationBuilder.AddCommandLine(Environment.GetCommandLineArgs(), Framework.SwitchMappings);
				var configuration = configurationBuilder.Build();

				string spiderTypeName = configuration["TYPE"];
				if (string.IsNullOrWhiteSpace(spiderTypeName))
				{
					Log.Logger.Error("未指定需要执行的爬虫类型");
					return;
				}

				var name = configuration["NAME"];
				var id = configuration["ID"] ?? Guid.NewGuid().ToString("N");
				var arguments = configuration["ARGS"]?.Split(' ');

				PrintEnvironment();

				var spiderTypes = DetectSpiders();

				if (spiderTypes == null || spiderTypes.Count == 0)
				{
					return;
				}

				var spiderType = spiderTypes.FirstOrDefault(x =>
					x.UnderlyingSystemType.ToString().ToLower() == spiderTypeName.ToLower());
				if (spiderType == null)
				{
					Log.Logger.Error($"未找到爬虫: {spiderTypeName}", 0, ConsoleColor.DarkYellow);
					return;
				}

				var builder = new SpiderHostBuilder();
				ConfigureService(configuration, builder);

				builder.Register(spiderType);
				var provider = builder.Build();
				var instance = provider.Create(spiderType);
				if (instance != null)
				{
					instance.Name = name;
					instance.Id = id;
					instance.RunAsync(arguments).ConfigureAwait(true).GetAwaiter().GetResult();
				}
				else
				{
					Log.Logger.Error("创建爬虫对象失败", 0, ConsoleColor.DarkYellow);
				}
			}
			catch (Exception e)
			{
				Log.Logger.Error($"执行失败: {e}");
			}
		}

		protected virtual void ConfigureSerialLog(string file)
		{
			var configure = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Verbose()
#else
				.MinimumLevel.Information()
#endif
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.WriteTo.Console().WriteTo
				.RollingFile(file);
			Log.Logger = configure.CreateLogger();
		}

		/// <summary>
		/// 检测爬虫类型
		/// </summary>
		/// <returns></returns>
		protected virtual HashSet<Type> DetectSpiders()
		{
			var spiderTypes = new HashSet<Type>();

			var spiderType = typeof(Spider);
			var asmNames = DetectAssemblies();

			foreach (var file in asmNames)
			{
				var asm = Assembly.Load(file);
				var types = asm.GetTypes();

				foreach (var type in types)
				{
					if (spiderType.IsAssignableFrom(type))
					{
						spiderTypes.Add(type);
					}
				}
			}

			if (asmNames.Count == 0)
			{
				var entryAsm = Assembly.GetEntryAssembly();
				if (entryAsm == null)
				{
					throw new SpiderException("未找到入口程序集");
				}

				asmNames = new List<string>
				{
					entryAsm.GetName(false).Name
				};
				var types = entryAsm.GetTypes();

				foreach (var type in types)
				{
					if (spiderType.IsAssignableFrom(type))
					{
						spiderTypes.Add(type);
					}
				}
			}

			var spiderInfo = $"检测到爬虫 : {spiderTypes.Count} 个";

			if (Environment.GetEnvironmentVariable("DOTNET_SPIDER_PRINT_SPIDERS") == "true")
			{
				spiderInfo = $"{spiderInfo}, {string.Join(", ", spiderTypes.Select(x => x.Name))}";
			}

			Log.Logger.Information($"程序集     : {string.Join(", ", asmNames)}", 0, ConsoleColor.DarkYellow);
			Log.Logger.Information(spiderInfo, 0, ConsoleColor.DarkYellow);

			return spiderTypes;
		}

		/// <summary>
		/// 扫描所有需要求的DLL
		/// </summary>
		/// <returns></returns>
		protected virtual List<string> DetectAssemblies()
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
			var files = Directory.GetFiles(path)
				.Where(f => f.EndsWith(".dll") || f.EndsWith(".exe"))
				.Select(f => Path.GetFileName(f).Replace(".dll", "").Replace(".exe", "")).ToList();
			return
				files.Where(f => !f.Contains("DotnetSpider")
				                 && DetectAssembles.Any(n => f.ToLower().Contains(n))).ToList();
		}

		private static void PrintEnvironment()
		{
			Framework.PrintInfo();
			var environmentVariables = Environment.GetEnvironmentVariables();
			var excludes = new List<string> {"PATH", "ASPNETCORE_URLS", "HOSTNAME", "DOTNET_RUNNING_IN_CONTAINER"};
			foreach (DictionaryEntry variable in environmentVariables)
			{
				if (!excludes.Contains(variable.Key))
				{
					Log.Logger.Information($"环境变量   : {variable.Key} {variable.Value}", 0, ConsoleColor.DarkYellow);
				}
			}

			var commands = string.Join(" ", Environment.GetCommandLineArgs());
			Log.Logger.Information($"运行参数   : {commands}", 0, ConsoleColor.DarkYellow);
			Log.Logger.Information($"运行目录   : {AppDomain.CurrentDomain.BaseDirectory}", 0,
				ConsoleColor.DarkYellow);
			Log.Logger.Information(
				$"操作系统   : {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "X64" : "X86")}", 0,
				ConsoleColor.DarkYellow);
		}
	}
}