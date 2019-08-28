using System;
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
		public static void Execute<TSpider>(params string[] args)
		{
			var logfile = Environment.GetEnvironmentVariable("DOTNET_SPIDER_ID");
			logfile = string.IsNullOrWhiteSpace(logfile) ? "dotnet-spider.log" : $"/logs/{logfile}.log";
			Environment.SetEnvironmentVariable("LOGFILE", logfile);

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

			Framework.SetMultiThread();

			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
			configurationBuilder.AddCommandLine(args, Framework.SwitchMappings);
			configurationBuilder.AddEnvironmentVariables();
			var configuration = configurationBuilder.Build();

			var id = configuration["DOTNET_SPIDER_ID"] ?? Guid.NewGuid().ToString("N");
			var name = configuration["DOTNET_SPIDER_NAME"] ?? id;
			var arguments = Environment.GetCommandLineArgs();

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

			var config = configuration["DOTNET_SPIDER_CONFIG"];
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

				x.AddCommandLine(Environment.GetCommandLineArgs(), Framework.SwitchMappings);
				x.AddEnvironmentVariables();
			});

			builder.ConfigureServices(services =>
			{
				services.AddLocalMessageQueue();
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
				Log.Logger.Error("Create spider object failed", 0, ConsoleColor.DarkYellow);
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
				Environment.SetEnvironmentVariable("LOGFILE", logfile);

				ConfigureSerialLog(logfile);

				Framework.SetMultiThread();

				var configurationBuilder = new ConfigurationBuilder();
				configurationBuilder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
				configurationBuilder.AddCommandLine(args, Framework.SwitchMappings);
				configurationBuilder.AddEnvironmentVariables();
				var configuration = configurationBuilder.Build();

				string spiderTypeName = configuration["DOTNET_SPIDER_TYPE"];
				if (string.IsNullOrWhiteSpace(spiderTypeName))
				{
					Log.Logger.Error("There is no specified spider type");
					return;
				}

				var name = configuration["DOTNET_SPIDER_NAME"];
				var id = configuration["DOTNET_SPIDER_ID"] ?? Guid.NewGuid().ToString("N");
				var arguments = configuration["DOTNET_SPIDER_ARGS"]?.Split(' ');

				var spiderTypes = DetectSpiders();

				if (spiderTypes == null || spiderTypes.Count == 0)
				{
					return;
				}

				var spiderType = spiderTypes.FirstOrDefault(x =>
					x.UnderlyingSystemType.ToString().ToLower() == spiderTypeName.ToLower());
				if (spiderType == null)
				{
					Log.Logger.Error($"Spider {spiderTypeName} not found", 0, ConsoleColor.DarkYellow);
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
					ConfigureSpider(instance);
					instance.RunAsync(arguments).ConfigureAwait(true).GetAwaiter().GetResult();
				}
				else
				{
					Log.Logger.Error("Create spider object failed", 0, ConsoleColor.DarkYellow);
				}
			}
			catch (Exception e)
			{
				Log.Logger.Error($"Execute spider failed: {e}");
			}
		}

		protected virtual void ConfigureSpider(Spider spider)
		{
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
					throw new SpiderException("EntryAssembly not found");
				}

				asmNames = new List<string> {entryAsm.GetName(false).Name};
				var types = entryAsm.GetTypes();

				foreach (var type in types)
				{
					if (spiderType.IsAssignableFrom(type))
					{
						spiderTypes.Add(type);
					}
				}
			}

			var spiderInfo = $"Spiders : {spiderTypes.Count}";

			if (Environment.GetEnvironmentVariable("DOTNET_SPIDER_PRINT_SPIDERS") == "true")
			{
				spiderInfo = $"{spiderInfo}, {string.Join(", ", spiderTypes.Select(x => x.Name))}";
			}

			Log.Logger.Information($"Assembly     : {string.Join(", ", asmNames)}", 0, ConsoleColor.DarkYellow);
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
	}
}
