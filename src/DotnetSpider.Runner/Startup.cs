using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
#if !NET_45
using Microsoft.Extensions.DependencyModel;
#endif

namespace DotnetSpider.Runner
{
	public class Startup
	{
		public static void Run(params string[] args)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine("Args: " + string.Join(" ", args));
			Console.WriteLine("");
			Console.ForegroundColor = ConsoleColor.White;

#if NETSTANDARD1_6
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
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
					Console.WriteLine("Please use command like: -s:[spider type name] -i:[identity] -a:[arg1,arg2...] -tid:[taskId]");
					Console.ForegroundColor = ConsoleColor.White;
					return;
				}
			}
			string spiderName = string.Empty;
			if (arguments.Count == 0 || !arguments.ContainsKey("-s") || !arguments.ContainsKey("-tid"))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("-s or -tid are necessary.");
				Console.ForegroundColor = ConsoleColor.White;
				return;
			}
			else
			{
				spiderName = arguments["-s"];
			}

#if !NET_45
			var deps = DependencyContext.Default;
#endif
			int totalTypesCount = 0;
			var spiders = new Dictionary<string, object>();
#if !NET_45
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

					if (hasNonParametersConstructor)
					{
						var interfaces = type.GetInterfaces();

						var isNamed = interfaces.Any(t => t.FullName == "DotnetSpider.Core.INamed");
						var isIdentity = interfaces.Any(t => t.FullName == "DotnetSpider.Core.IIdentity");
						var isRunnable = interfaces.Any(t => t.FullName == "DotnetSpider.Core.IRunable");
						var isTask = interfaces.Any(t => t.FullName == "DotnetSpider.Extension.ITask");

						if (isNamed && isRunnable && isIdentity && isTask)
						{
							var property = type.GetProperties().First(p => p.Name == "Name");
							object runner = Activator.CreateInstance(type);
							var name = (string)property.GetValue(runner);
							if (!spiders.ContainsKey(name))
							{
								Console.WriteLine($"Detected spider: {name}.");
								spiders.Add(name, runner);
							}
							else
							{
								Console.ForegroundColor = ConsoleColor.Red;
								Console.WriteLine();
								Console.WriteLine($"Spider {name} are duplicate.");
								Console.WriteLine();
								Console.ForegroundColor = ConsoleColor.White;
								return;
							}
							++totalTypesCount;
						}
					}
				}
			}

			if (spiders.Count == 0)
			{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine();
				Console.WriteLine("Did not detect any spider.");
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.White;
				return;
			}

			if (!spiders.ContainsKey(spiderName))
			{
				Console.WriteLine($"There is no spider named: {spiderName}.");
				return;
			}
			var spider = spiders[spiderName];
			if (arguments.ContainsKey("-i"))
			{
				var property = spider.GetType().GetProperties().First(p => p.Name == "Identity");
				if (arguments["-i"].ToLower() != "guid")
				{
					property.SetValue(spider, arguments["-i"]);
				}
				else
				{
					property.SetValue(spider, Guid.NewGuid().ToString("N"));
				}
			}

			if (arguments.ContainsKey("-tid"))
			{
				var property = spider.GetType().GetProperties().First(p => p.Name == "TaskId");
				if (arguments["-tid"].ToLower() != "guid")
				{
					property.SetValue(spider, arguments["-tid"]);
				}
				else
				{
					property.SetValue(spider, Guid.NewGuid().ToString("N"));
				}
			}

			var method = spider.GetType().GetMethod("Run");
			if (!arguments.ContainsKey("-a"))
			{
				method.Invoke(spider, new object[] { new string[] { } });
			}
			else
			{
				method.Invoke(spider, new object[] { new string[] { arguments["-a"] } });
			}
		}
#if NET_45
		private static List<string> DetectDlls()
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
			return Directory.GetFiles(path).Where(f => f.ToLower().EndsWith("dotnetspider.sample.exe") || f.ToLower().EndsWith("dotnetspider.sample.dll") || f.ToLower().EndsWith("spiders.dll") || f.ToLower().EndsWith("spiders.exe") || f.ToLower().EndsWith("crawlers.dll") || f.ToLower().EndsWith("crawlers.exe")).ToList();
		}
#endif
	}
}