using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotnetSpider.Core.Infrastructure;
using CommandLine;
using DotnetSpider.Common;
#if NETSTANDARD
using System.Text;
// ReSharper disable UnusedAutoPropertyAccessor.Global
#endif

namespace DotnetSpider.Core
{
	public class Options
	{
		[Option('s', "spider", Required = true, HelpText = "Which spider you want to run.")]
		public string Spider { get; set; }

		[Option('n', "name", Required = false, HelpText = "The name of spider.")]
		public string Name { get; set; }

		[Option('t', "tid", Required = false, HelpText = "The task id of spider.")]
		public string TaskId { get; set; }

		[Option('a', "arguments", Required = false, HelpText = "The extra arguments to run spider.")]
		public string Arguments { get; set; }

		[Option('i', "identity", Required = false, HelpText = "The identity of spider.")]
		public string Identity { get; set; }

		[Option('c', "config", Required = false, HelpText = "The config file you want to use.")]
		public string Config { get; set; }

		public string[] GetArguments()
		{
			return string.IsNullOrEmpty(Arguments) ? new string[0] : Arguments.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		}
	}

	/// <summary>
	/// 启动任务工具
	/// </summary>
	public static class Startup
	{
		/// <summary>
		/// DLL名字中包含任意一个即是需要扫描的DLL
		/// </summary>
		public static List<string> DetectNames = new List<string> { "dotnetspider.sample", "crawler", "crawlers", "spider", "spiders" };

		static Startup()
		{
			LogUtil.Init();
		}

		/// <summary>
		/// 运行
		/// </summary>
		/// <param name="args">运行参数</param>
		public static void Run(params string[] args)
		{
			var options = Parse(args);
			if (options != null)
			{
				SetEncoding();

				PrintEnviroment(args);

				LoadConfiguration(options.Config);

				var spiderName = options.Spider;

				var spiderTypes = DetectSpiders();

				if (spiderTypes == null || spiderTypes.Count == 0)
				{
					return;
				}

				var spider = CreateSpiderInstance(spiderName, options, spiderTypes);
				if (spider != null)
				{
					PrintInfo.PrintLine();

					var runMethod = spiderTypes[spiderName].GetMethod("Run");

					if (runMethod != null) runMethod.Invoke(spider, new object[] { options.GetArguments() });
				}
			}
		}


#if NET40
		public static Options Parse(params string[] args)
		{
			var arguments = new List<string>();

			foreach (var arg in args)
			{
				if (arg.Contains(":"))
				{
					var array = arg.Split(':').Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToList();
					if (array.Count == 1)
					{
						array.Add("");
					}
					arguments.AddRange(array);
				}
				else
				{
					arguments.Add(arg);
				}
			}

			Parser parser = new Parser(config =>
			{
				config.CaseSensitive = false;
			});

			var options = new Options();
			if (parser.ParseArguments(arguments.ToArray(), options))
			{
				return options;
			}
			return null;
		}
#else
		public static Options Parse(params string[] args)
		{
			var arguments = new List<string>();

			foreach (var arg in args)
			{
				var array = arg.Split(':').Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToList();
				if (array.Count == 1)
				{
					array.Add("");
				}
				arguments.AddRange(array);
			}

			Parser parser = new Parser(config =>
			{
				config.CaseSensitive = false;
				config.EnableDashDash = false;
				config.CaseInsensitiveEnumValues = false;
			});

			var result = parser.ParseArguments<Options>(arguments);
			if (result.Tag == ParserResultType.Parsed)
			{
				if (result is Parsed<Options> parsed) return parsed.Value;
			}
			return null;
		}
#endif

		public static void LoadConfiguration(string config)
		{
			if (!string.IsNullOrWhiteSpace(config))
			{
				Env.LoadConfiguration(config);
			}
		}

		/// <summary>
		/// 检测爬虫类型
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, Type> DetectSpiders()
		{
			var spiderTypes = new Dictionary<string, Type>();
			var namedType = typeof(INamed);
			var identityType = typeof(IIdentity);
			var irunableType = typeof(IRunable);
			foreach (var file in DetectDlls())
			{
				var asm = Assembly.Load(file);
				var types = asm.GetTypes();

				Console.WriteLine($"Fetch assembly   : {asm.GetName(false)}.");

				foreach (var type in types)
				{
					bool hasNonParametersConstructor = type.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Length == 0);

					var fullName = type.FullName;
					if (string.IsNullOrWhiteSpace(fullName))
					{
						continue;
					}

					if (hasNonParametersConstructor)
					{
						var interfaces = type.GetInterfaces();

						var isNamed = interfaces.Any(t => namedType.IsAssignableFrom(t));
						var isIdentity = interfaces.Any(t => identityType.IsAssignableFrom(t));
						var isRunnable = interfaces.Any(t => irunableType.IsAssignableFrom(t));

						if (isNamed && isRunnable && isIdentity)
						{
							if (!spiderTypes.ContainsKey(fullName))
							{
								spiderTypes.Add(fullName, type);
							}
							else
							{
								ConsoleHelper.WriteLine($"Spider {type.Name} are duplicate.", 1);
								return null;
							}

							var startupName = type.GetCustomAttributes(typeof(TaskName), true).FirstOrDefault() as TaskName;
							if (startupName != null)
							{
								if (!spiderTypes.ContainsKey(startupName.Name))
								{
									spiderTypes.Add(startupName.Name, type);
								}
								else
								{
									ConsoleHelper.WriteLine($"Spider {type.Name} are duplicate.", 1);
									return null;
								}
							}
						}
					}
				}
			}

			if (spiderTypes.Count == 0)
			{
				ConsoleHelper.WriteLine("Did not detect any spider.", 1, ConsoleColor.DarkYellow);
				return null;
			}

			Console.WriteLine($"Count of crawlers: {spiderTypes.Keys.Count}");

			return spiderTypes;
		}

		/// <summary>
		/// 扫描所有需要求的DLL
		/// </summary>
		/// <returns></returns>
		public static List<string> DetectDlls()
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
			var files = Directory.GetFiles(path)
				.Where(f => f.EndsWith(".dll") || f.EndsWith(".exe"))
				.Select(f => Path.GetFileName(f).Replace(".dll", "").Replace(".exe", "")).ToList();
			return
				files.Where(f => !f.EndsWith("DotnetSpider.HtmlAgilityPack.Css")
				&& !f.EndsWith("DotnetSpider.Common")
				&& !f.EndsWith("DotnetSpider.Proxy")
				&& !f.EndsWith("DotnetSpider.Downloader")
				&& !f.EndsWith("DotnetSpider.Extraction")
				&& !f.EndsWith("DotnetSpider.Extension")
				&& !f.EndsWith("DotnetSpider.Core")
				&& DetectNames.Any(n => f.ToLower().Contains(n))).ToList();
		}

		/// <summary>
		/// 反射爬虫对象
		/// </summary>
		/// <param name="spiderName">名称</param>
		/// <param name="options">运行参数</param>
		/// <param name="spiderTypes">所有的爬虫类型</param>
		/// <returns>爬虫对象</returns>
		public static object CreateSpiderInstance(string spiderName, Options options, Dictionary<string, Type> spiderTypes)
		{
			if (!spiderTypes.ContainsKey(spiderName))
			{
				ConsoleHelper.WriteLine($"Spider: {spiderName} unfound.", ConsoleColor.DarkYellow);
				return null;
			}
			var spiderType = spiderTypes[spiderName];

			var spider = Activator.CreateInstance(spiderType);
			var spiderProperties = spiderType.GetProperties();

			if (!string.IsNullOrWhiteSpace(options.Identity))
			{
				var identity = "guid" == options.Identity.ToLower() ? Guid.NewGuid().ToString("N") : options.Identity.Trim();
				if (!string.IsNullOrWhiteSpace(identity))
				{
					var property = spiderProperties.First(p => p.Name == "Identity");
					property.SetValue(spider, identity, new object[0]);
				}
			}

			if (!string.IsNullOrWhiteSpace(options.TaskId))
			{
				var property = spiderProperties.FirstOrDefault(p => p.Name == "TaskId");
				if (property != null)
				{
					var taskId = "guid" == options.TaskId.ToLower() ? Guid.NewGuid().ToString("N") : options.TaskId.Trim();
					if (!string.IsNullOrWhiteSpace(taskId))
					{
						property.SetValue(spider, taskId, new object[0]);
					}
				}
			}

			if (!string.IsNullOrWhiteSpace(options.Name))
			{
				var property = spiderProperties.First(p => p.Name == "Name");
				property.SetValue(spider, options.Name.Trim(), new object[0]);
			}

			return spider;
		}

		private static void PrintEnviroment(params string[] args)
		{
			Console.WriteLine("");
			PrintInfo.Print();
			var commands = string.Join(" ", args);
			PrintInfo.PrintLine();
			Console.WriteLine($"Args             : {commands}");
			Console.WriteLine($"BaseDirectory    : {AppDomain.CurrentDomain.BaseDirectory}");
			Console.WriteLine($"System           : {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "X64" : "X86")}");
		}

		private static void SetEncoding()
		{
#if NETSTANDARD
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
		}
	}
}