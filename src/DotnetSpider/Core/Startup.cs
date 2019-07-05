using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 启动任务工具
	/// </summary>
	public static class Startup
	{
		/// <summary>
		/// DLL 名字中包含任意一个即是需要扫描的 DLL
		/// </summary>
		public static readonly List<string> DetectAssembles = new List<string> {"dotnetspider.sample"};

		/// <summary>
		/// 运行
		/// </summary>
		/// <param name="args">运行参数</param>
		public static void Run(params string[] args)
		{
			Framework.SetEncoding();

			var configurationBuilder = Framework.CreateConfigurationBuilder(null, args);
			var configuration = configurationBuilder.Build();
			string spider = configuration["spider"];
			if (string.IsNullOrWhiteSpace(spider))
			{
				throw new SpiderException("未指定需要执行的爬虫");
			}

			var name = configuration["name"];
			var id = configuration["id"] ?? Guid.NewGuid().ToString("N");
			var config = configuration["config"];
			var arguments = configuration["args"]?.Split(' ');
			var distribute = configuration["distribute"] == "true";

			PrintEnvironment(args);

			var spiderTypes = DetectSpiders();

			if (spiderTypes == null || spiderTypes.Count == 0)
			{
				return;
			}

			var spiderType = spiderTypes.FirstOrDefault(x => x.Name.ToLower() == spider.ToLower());
			if (spiderType == null)
			{
				ConsoleHelper.WriteLine($"未找到爬虫: {spider}", 0, ConsoleColor.DarkYellow);
				return;
			}

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
			builder.ConfigureAppConfiguration(b =>
			{
				// 添加 JSON 配置文件
				b.AddJsonFile(config);
				b.AddCommandLine(args);
			});

			if (!distribute)
			{
				builder.ConfigureServices(b =>
				{
					b.AddLocalEventBus();
					b.AddLocalDownloadCenter();
					b.AddDownloaderAgent(x =>
					{
						x.UseFileLocker();
						x.UseDefaultAdslRedialer();
						x.UseDefaultInternetDetector();
					});
					b.AddStatisticsCenter(x =>
					{
						// 添加内存统计服务
						x.UseMemory();
					});
				});
			}

			builder.Register(spiderType);
			var provider = builder.Build();
			var instance = provider.Create(spiderType);
			if (instance != null)
			{
				instance.Name = name;
				instance.Id = id;
				instance.RunAsync(arguments).ConfigureAwait(false).GetAwaiter();
			}
			else
			{
				ConsoleHelper.WriteLine("创建爬虫对象失败", 0, ConsoleColor.DarkYellow);
			}
		}

		/// <summary>
		/// 检测爬虫类型
		/// </summary>
		/// <returns></returns>
		private static HashSet<Type> DetectSpiders()
		{
			var spiderTypes = new HashSet<Type>();

			var spiderType = typeof(Spider);
			var asmNames = new List<string>();
			foreach (var file in DetectAssemblies())
			{
				var asm = Assembly.Load(file);
				var types = asm.GetTypes();
				asmNames.Add(asm.GetName(false).Name);

				foreach (var type in types)
				{
					if (spiderType.IsAssignableFrom(type))
					{
						spiderTypes.Add(type);
					}
				}
			}

			ConsoleHelper.WriteLine($"程序集     : {string.Join(", ", asmNames)}", 0, ConsoleColor.DarkYellow);
			ConsoleHelper.WriteLine($"检测到爬虫 : {spiderTypes.Count} 个", 0, ConsoleColor.DarkYellow);

			return spiderTypes;
		}

		/// <summary>
		/// 扫描所有需要求的DLL
		/// </summary>
		/// <returns></returns>
		private static List<string> DetectAssemblies()
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
			var files = Directory.GetFiles(path)
				.Where(f => f.EndsWith(".dll") || f.EndsWith(".exe"))
				.Select(f => Path.GetFileName(f).Replace(".dll", "").Replace(".exe", "")).ToList();
			return
				files.Where(f => !f.EndsWith("DotnetSpider")
				                 && DetectAssembles.Any(n => f.ToLower().Contains(n))).ToList();
		}

		private static void PrintEnvironment(params string[] args)
		{
			Framework.PrintInfo();
			var commands = string.Join(" ", args);
			ConsoleHelper.WriteLine($"运行参数   : {commands}", 0, ConsoleColor.DarkYellow);
			ConsoleHelper.WriteLine($"运行目录   : {AppDomain.CurrentDomain.BaseDirectory}", 0,
				ConsoleColor.DarkYellow);
			ConsoleHelper.WriteLine(
				$"操作系统   : {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "X64" : "X86")}", 0,
				ConsoleColor.DarkYellow);
		}
	}
}