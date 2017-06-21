using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if !NET_45
using Microsoft.Extensions.DependencyModel;
#endif

namespace DotnetSpider.Runner
{
	public class Startup
	{
		public static void Run(params string[] args)
		{
			Dictionary<string, string> arguments = new Dictionary<string, string>();
			foreach (var arg in args)
			{
				var results = arg.Split(':');
				if (results.Length == 2)
				{
					if (arguments.ContainsKey(results[0]))
					{
						arguments[results[0]] = results[1];
					}
					else
					{
						arguments.Add(results[0], results[1]);
					}
				}
				else if (results.Length == 1)
				{
					if (!arguments.ContainsKey(results[0]))
					{
						arguments.Add(results[0], string.Empty);
					}
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Please use command like: -s:[spider type name] -b:[batch] -a:[arg1,arg2...]");
					Console.ForegroundColor = ConsoleColor.White;
					return;
				}
			}
			string spiderName = string.Empty;
			if (arguments.Count == 0 || !arguments.ContainsKey("-s"))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("-s is necessary.");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Press any key to continue...");
				Console.Read();
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
			foreach (var library in deps.CompileLibraries.Where(l => l.Name.ToLower().EndsWith("dotnetspider.sample") || l.Name.ToLower().EndsWith("spiders.dll") || l.Name.ToLower().EndsWith("spiders.exe")))
			{
				var asm = Assembly.Load(new AssemblyName(library.Name));
				var types = asm.GetTypes();
#else
			foreach (var asm in  AppDomain.CurrentDomain.GetAssemblies().Where(l => l.GetName().Name.ToLower().EndsWith("dotnetspider.sample") || l.GetName().Name.ToLower().EndsWith("spiders.dll") || l.GetName().Name.ToLower().EndsWith("spiders.exe")))
			{
				var types = asm.GetTypes();
#endif

				foreach (var type in types)
				{
					bool hasNonParametersConstructor = type.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Length == 0);

					if (hasNonParametersConstructor)
					{
						var interfaces = type.GetInterfaces();

						var isNamed = interfaces.Any(t => t.FullName == "DotnetSpider.Core.INamed");
						var isIdentity = interfaces.Any(t => t.FullName == "DotnetSpider.Core.IIdentity");
						var isRunnable = interfaces.Any(t => t.FullName == "DotnetSpider.Core.IRunable");

						if (isNamed && isRunnable && isIdentity)
						{
							var property = type.GetProperties().First(p => p.Name == "Name");
							object runner = Activator.CreateInstance(type);
							var name = (string)property.GetValue(runner);
							if (!spiders.ContainsKey(name))
							{
								spiders.Add(name, runner);
							}
							++totalTypesCount;
						}
					}
				}
			}

			if (spiders.Count == 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("No spiders.");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Press any key to continue...");
				Console.Read();
				return;
			}

			if (spiders.Count != totalTypesCount)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("There are some duplicate spiders.");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Press any key to continue...");
				Console.Read();
				return;
			}

			if (!spiders.ContainsKey(spiderName))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"There is no spider named: {spiderName}.");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Press any key to continue...");
				Console.Read();
				return;
			}

			if (arguments.ContainsKey("-i"))
			{
				var property = spiders[spiderName].GetType().GetProperties().First(p => p.Name == "Identity");
				property.SetValue(spiders[spiderName], arguments["-i"]);
			}

			var method = spiders[spiderName].GetType().GetMethod("Run");
			if (!arguments.ContainsKey("-a"))
			{
				method.Invoke(spiders[spiderName], new object[] { new string[] { } });
			}
			else
			{
				method.Invoke(spiders[spiderName], new object[] { new string[] { arguments["-a"] } });
			}
		}

		private static List<string> DetectDlls()
		{
#if !NET_45
			var path = Path.Combine(AppContext.BaseDirectory);
#else
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
#endif
			//return new List<string> { Path.Combine(path, "DotnetSpider.Core.dll") };
			return Directory.GetFiles(path).Where(f => f.ToLower().EndsWith("dotnetspider.sample") || f.ToLower().EndsWith("dotnetspider.sample.dll") || f.ToLower().EndsWith("Spiders.dll") || f.ToLower().EndsWith("Spiders.exe")).ToList();
		}
	}
}