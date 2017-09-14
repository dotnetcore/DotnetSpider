using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
#if NET_CORE
using Microsoft.Extensions.DependencyModel;
using System.Runtime.InteropServices;
using System.Text;
#endif

namespace DotnetSpider.Core
{
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
				var spiderType = spiderTypes[spiderName];

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
				var valuePairs = arguments["-e"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var pair in valuePairs)
				{
					var datas = pair.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
					if (datas.Length == 2)
					{
						AppDomain.CurrentDomain.SetData(datas[0], datas[1]);
					}
				}
			}
		}

		public static object CreateSpiderInstance(string spiderName, Dictionary<string, string> arguments, Dictionary<string, Type> spiderTypes)
		{
			if (!spiderTypes.ContainsKey(spiderName))
			{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine($"Spider: {spiderName} unfound.");
				Console.ForegroundColor = ConsoleColor.White;
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

				Console.WriteLine($"Fetch assembly: {asm.FullName}.");

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
								Console.ForegroundColor = ConsoleColor.Red;
								Console.WriteLine();
								Console.WriteLine($"Spider {type.Name} are duplicate.");
								Console.WriteLine();
								Console.ForegroundColor = ConsoleColor.White;
								return null;
							}
						}
					}
				}
			}

			if (spiderTypes.Count == 0)
			{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine();
				Console.WriteLine("Did not detect any spider.");
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.White;
				return null;
			}

			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"Detected {spiderTypes.Keys.Count} crawlers.");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine();

			//Environment.PrintLine('=');

			return spiderTypes;
		}

		public static Dictionary<string, string> AnalyzeArguments(params string[] args)
		{
			Dictionary<string, string> arguments = new Dictionary<string, string>();
			foreach (var arg in args)
			{
				if (string.IsNullOrEmpty(arg) || string.IsNullOrWhiteSpace(arg))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Command: -s:[spider type name] -i:[identity] -a:[arg1,arg2...] -tid:[taskId] -n:[name] -e:[en1=value1,en2=value2,...]");
					Console.ForegroundColor = ConsoleColor.White;
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
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Command: -s:[spider type name] -i:[identity] -a:[arg1,arg2...] -tid:[taskId] -n:[name]");
						Console.ForegroundColor = ConsoleColor.White;
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
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Command: -s:[spider type name] -i:[identity] -a:[arg1,arg2...] -tid:[taskId] -n:[name]");
						Console.ForegroundColor = ConsoleColor.White;
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
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Error: -s & -tid are necessary.");
				Console.ForegroundColor = ConsoleColor.White;
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
			Core.Env.PrintInfo();
			Console.WriteLine("");
			Console.ForegroundColor = ConsoleColor.Cyan;
			var commands = string.Join(" ", args);
			Console.WriteLine($"Args:           {commands}");
			Console.WriteLine($"BaseDirectory:  {Env.BaseDirectory}");
			Console.WriteLine($"System:         {System.Environment.OSVersion} {(System.Environment.Is64BitOperatingSystem ? "X64" : "X86")}");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("");
		}

		private static void SetConsoleEncoding()
		{
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
		}
	}
}