using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if NET_CORE
using Microsoft.Extensions.DependencyModel;
using System.Runtime.InteropServices;
using System.Text;
#endif

namespace DotnetSpider.Core
{
	public class Startup
	{
		public static void Run(params string[] args)
		{
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
			Console.WriteLine("");
			Spider.PrintInfo();
			Console.WriteLine("");
			Console.ForegroundColor = ConsoleColor.Cyan;
			var commands = string.Join(" ", args);
			Console.WriteLine($"Args:           {commands}");
			Console.WriteLine($"BaseDirectory:  {Environment.BaseDirectory}");
			Console.WriteLine($"System:         {System.Environment.OSVersion} {(System.Environment.Is64BitOperatingSystem ? "X64" : "X86")}");

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("");

			Dictionary<string, string> arguments = new Dictionary<string, string>();
			foreach (var arg in args)
			{
				var results = arg.Split(':');
				if (results.Length == 2)
				{
					var key = results[0].Trim();
					if (arguments.ContainsKey(key))
					{
						arguments[key] = results[1].Trim();
					}
					else
					{
						arguments.Add(key, results[1].Trim());
					}
				}
				else if (results.Length == 1)
				{
					var key = results[0].Trim();
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
					return;
				}
			}

			if (arguments.Count == 0 || !arguments.ContainsKey("-s") || !arguments.ContainsKey("-tid"))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Error: -s & -tid are necessary.");
				Console.ForegroundColor = ConsoleColor.White;
				return;
			}

			var spiderName = arguments["-s"];

#if NET_CORE
			var deps = DependencyContext.Default;
#endif

			var spiderTypes = new Dictionary<string, Type>();
#if NET_CORE
			foreach (var library in deps.CompileLibraries.Where(l => l.Name.ToLower().EndsWith("dotnetspider.sample") || l.Name.ToLower().EndsWith("spiders") || l.Name.ToLower().EndsWith("crawlers")))
			{
				var asm = Assembly.Load(new AssemblyName(library.Name));
				var types = asm.GetTypes();
#else
			foreach (var file in DetectDlls())
			{
				var asm = Assembly.LoadFrom(file);
				var types = asm.GetTypes();
#endif
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
								return;
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
				return;
			}

			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"Detected {spiderTypes.Keys.Count} crawlers.");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine();
			Console.WriteLine("=================================================================");
			Console.WriteLine();

			if (!spiderTypes.ContainsKey(spiderName))
			{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine($"Spider: {spiderName} unfound.");
				Console.ForegroundColor = ConsoleColor.White;
				return;
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
					property.SetValue(spider, arguments["-i"]);
				}
			}

			if (arguments.ContainsKey("-tid"))
			{
				var property = spiderProperties.First(p => p.Name == "TaskId");
				var taskId = "guid" == arguments["-tid"].ToLower() ? Guid.NewGuid().ToString("N") : arguments["-tid"].Trim();
				if (!string.IsNullOrEmpty(taskId) && !string.IsNullOrWhiteSpace(taskId))
				{
					property.SetValue(spider, taskId);
				}
			}

			if (arguments.ContainsKey("-n"))
			{
				var property = spiderProperties.First(p => p.Name == "Name");
				if (!string.IsNullOrEmpty(arguments["-n"]) && string.IsNullOrWhiteSpace(arguments["-n"]))
				{
					property.SetValue(spider, arguments["-n"].Trim());
				}
			}

			var runMethod = spiderType.GetMethod("Run");

			if (!arguments.ContainsKey("-a"))
			{
				runMethod.Invoke(spider, new object[] { new string[] { } });
			}
			else
			{
				var parameters = arguments["-a"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (parameters.Contains("report"))
				{
					spiderType.GetProperty("EmptySleepTime")?.SetValue(spider, 1000);
				}

				runMethod.Invoke(spider, new object[] { parameters });
			}
		}

#if !NET_CORE
		private static List<string> DetectDlls()
		{
			var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
			return System.IO.Directory.GetFiles(path).Where(f => f.ToLower().EndsWith("dotnetspider.sample.exe") || f.ToLower().EndsWith("dotnetspider.sample.dll") || f.ToLower().EndsWith("spiders.dll") || f.ToLower().EndsWith("spiders.exe") || f.ToLower().EndsWith("crawlers.dll") || f.ToLower().EndsWith("crawlers.exe")).ToList();
		}
#endif
	}
}