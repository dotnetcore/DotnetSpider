using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DotnetSpider.Core.Infrastructure;
#if NET_CORE
using Microsoft.Extensions.DependencyModel;
using System.Runtime.InteropServices;
using System.Text;
#endif

namespace DotnetSpider.Core
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class TaskName : Attribute
	{
		public string Name
		{
			get;
			private set;
		}

		public TaskName(string name)
		{
			Name = name;
		}
	}

	public static class Startup
	{
		public static List<string> DetectNames = new List<string> { "dotnetspider.sample", "crawler", "crawlers", "spider", "spiders" };

		public static void Run(params string[] args)
		{
			SetConsoleEncoding();

			PrintEnviroment(args);

			Dictionary<string, string> arguments = AnalyzeArguments(args);

			if (arguments == null || arguments.Count == 0)
			{
				return;
			}

			SetEnviroment(arguments);

			var spiderName = arguments["-s"];

			var spiderTypes = DetectSpiders();

			if (spiderTypes == null || spiderTypes.Count == 0)
			{
				return;
			}

			var spider = CreateSpiderInstance(spiderName, arguments, spiderTypes);
			if (spider != null)
			{
				var runMethod = spiderTypes[spiderName].GetMethod("Run");

				if (!arguments.ContainsKey("-a"))
				{
					runMethod.Invoke(spider, new object[] { new string[] { } });
				}
				else
				{
					var parameters = arguments["-a"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					runMethod.Invoke(spider, new object[] { parameters });
				}
			}
		}

		public static void SetEnviroment(Dictionary<string, string> arguments)
		{
			if (arguments.ContainsKey("-e"))
			{
				var valuePairs = arguments["-e"].Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var pair in valuePairs)
				{
					var datas = pair.Split(new [] { '=' }, StringSplitOptions.RemoveEmptyEntries);
					if (datas.Length == 2)
					{
						AppDomain.CurrentDomain.SetData(datas[0], datas[1]);
					}
				}

				Env.Reload();
			}
		}

		public static object CreateSpiderInstance(string spiderName, Dictionary<string, string> arguments, Dictionary<string, Type> spiderTypes)
		{
			if (!spiderTypes.ContainsKey(spiderName))
			{
				ConsoleHelper.WriteLine($"Spider: {spiderName} unfound.", ConsoleColor.DarkYellow);
				return null;
			}
			var spiderType = spiderTypes[spiderName];

			var spider = Activator.CreateInstance(spiderType);
			var spiderProperties = spiderType.GetProperties();

			if (arguments.ContainsKey("-i"))
			{
				var identity = "guid" == arguments["-i"].ToLower() ? Guid.NewGuid().ToString("N") : arguments["-i"];
				if (!string.IsNullOrEmpty(identity) && !string.IsNullOrWhiteSpace(identity))
				{
					var property = spiderProperties.First(p => p.Name == "Identity");
					property.SetValue(spider, identity);
				}
			}

			if (arguments.ContainsKey("-tid"))
			{
				var property = spiderProperties.FirstOrDefault(p => p.Name == "TaskId");
				if (property != null)
				{
					var taskId = "guid" == arguments["-tid"].ToLower() ? Guid.NewGuid().ToString("N") : arguments["-tid"].Trim();
					if (!string.IsNullOrEmpty(taskId) && !string.IsNullOrWhiteSpace(taskId))
					{
						property.SetValue(spider, taskId);
					}
				}
			}

			if (arguments.ContainsKey("-n"))
			{
				var property = spiderProperties.First(p => p.Name == "Name");
				if (!string.IsNullOrEmpty(arguments["-n"]) && !string.IsNullOrWhiteSpace(arguments["-n"]))
				{
					property.SetValue(spider, arguments["-n"].Trim());
				}
			}

			if (arguments.ContainsKey("-a"))
			{
				var parameters = arguments["-a"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (parameters.Contains("report"))
				{
					spiderType.GetProperty("EmptySleepTime")?.SetValue(spider, 1000);
				}
			}

			return spider;
		}

		public static Dictionary<string, Type> DetectSpiders()
		{
			var spiderTypes = new Dictionary<string, Type>();

			foreach (var file in DetectDlls())
			{
				var asm = Assembly.Load(file);
				var types = asm.GetTypes();

				Console.WriteLine($"Fetch assembly   : {asm.GetName(false)}.");

				foreach (var type in types)
				{
					bool hasNonParametersConstructor = type.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Length == 0);

					var fullName = type.FullName;
					if (string.IsNullOrEmpty(fullName))
					{
						continue;
					}

					if (hasNonParametersConstructor)
					{
						var interfaces = type.GetInterfaces();

						var isNamed = interfaces.Any(t => t.FullName == "DotnetSpider.Core.INamed");
						var isIdentity = interfaces.Any(t => t.FullName == "DotnetSpider.Core.IIdentity");
						var isRunnable = interfaces.Any(t => t.FullName == "DotnetSpider.Core.IRunable");

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

							var startupName = type.GetCustomAttribute<TaskName>();
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
			Console.WriteLine();

			PrintInfo.PrintLine();
			return spiderTypes;
		}

		public static Dictionary<string, string> AnalyzeArguments(params string[] args)
		{
			Dictionary<string, string> arguments = new Dictionary<string, string>();
			foreach (var arg in args)
			{
				if (string.IsNullOrEmpty(arg) || string.IsNullOrWhiteSpace(arg))
				{
					ConsoleHelper.WriteLine("Command: -s:[spider type name] -i:[identity] -a:[arg1,arg2...] -tid:[taskId] -n:[name] -e:[en1=value1,en2=value2,...]");
					return null;
				}

				var results = arg.Replace(" ", "").Split(':');
				if (results.Length == 2)
				{
					var key = results[0].Trim();
					if (Regex.IsMatch(key, @"-\w+"))
					{
						if (arguments.ContainsKey(key))
						{
							arguments[key] = results[1].Trim();
						}
						else
						{
							arguments.Add(key, results[1].Trim());
						}
					}
					else
					{
						ConsoleHelper.WriteLine("Command: -s:[spider type name] -i:[identity] -a:[arg1,arg2...] -tid:[taskId] -n:[name]");
						return null;
					}
				}
				else if (results.Length == 1)
				{
					var key = results[0].Trim();
					if (Regex.IsMatch(key, @"-\w+"))
					{
						if (!arguments.ContainsKey(key))
						{
							arguments.Add(key, string.Empty);
						}
					}
					else
					{
						ConsoleHelper.WriteLine("Command: -s:[spider type name] -i:[identity] -a:[arg1,arg2...] -tid:[taskId] -n:[name]");
						return null;
					}
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Command: -s:[spider type name] -i:[identity] -a:[arg1,arg2...] -tid:[taskId] -n:[name]");
					Console.ForegroundColor = ConsoleColor.White;
					return null;
				}
			}

			if (arguments.Count == 0 || !arguments.ContainsKey("-s") || !arguments.ContainsKey("-tid"))
			{
				ConsoleHelper.WriteLine("Error: -s & -tid are necessary.");
				return null;
			}

			return arguments;
		}

		public static List<string> DetectDlls()
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
			return Directory.GetFiles(path).Where(f => f.EndsWith(".dll")).Select(f => Path.GetFileName(f).Replace(".dll", "")).Where(f => !f.EndsWith("DotnetSpider.HtmlAgilityPack.Css") && !f.EndsWith("DotnetSpider.Extension") && !f.EndsWith("DotnetSpider2.Extension") && !f.EndsWith("DotnetSpider.Core") && !f.EndsWith("DotnetSpider2.Core") && DetectNames.Any(n => f.ToLower().Contains(n))).ToList();
		}

		private static void PrintEnviroment(params string[] args)
		{
			Console.WriteLine("");
			PrintInfo.Print();
			var commands = string.Join(" ", args);
			Console.WriteLine($"Args             : {commands}");
			Console.WriteLine($"BaseDirectory    : {AppDomain.CurrentDomain.BaseDirectory}");
			Console.WriteLine($"System           : {Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "X64" : "X86")}");
		}

		private static void SetConsoleEncoding()
		{
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
		}
	}
}