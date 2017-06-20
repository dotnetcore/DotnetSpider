using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
#if NETCOREAPP1_1
using System.Runtime.Loader;
#endif

namespace DotnetSpider.Runner
{
	public class Runner
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

#if NETCOREAPP1_1
			var asl = AssemblyLoadContext.Default;
			asl.Resolving += Asl_Resolving;
#endif
			int totalTypesCount = 0;
			var spiders = new Dictionary<string, object>();
			foreach (var spiderDll in DetectDlls())
			{
#if NETCOREAPP1_1
				var asm = asl.LoadFromAssemblyPath(Path.Combine(AppContext.BaseDirectory, spiderDll));
				var types = asm.GetTypes();
#else

				var asm = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, spiderDll));
				var types = asm.GetTypes();
#endif

				foreach (var type in types)
				{
					bool hasNonParametersConstructor = type.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Length == 0);

					if (hasNonParametersConstructor)
					{
						var interfaces = type.GetInterfaces();

						var isNamed = interfaces.Any(t => t.FullName == "DotnetSpider.Core.INamed");
						var isRunner = interfaces.Any(t => t.FullName == "DotnetSpider.Core.IRunable");
						var isBatch = interfaces.Any(t => t.FullName == "DotnetSpider.Core.IBatch");

						if (isNamed && isRunner && isBatch)
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

			if (arguments.ContainsKey("-b"))
			{
				var property = spiders[spiderName].GetType().GetProperties().First(p => p.Name == "Batch");
				property.SetValue(spiders[spiderName], arguments["-b"]);
			}
			if (arguments.ContainsKey("-n"))
			{
				var property = spiders[spiderName].GetType().GetProperties().First(p => p.Name == "Name");
				property.SetValue(spiders[spiderName], arguments["-n"]);
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

		private static Assembly Asl_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
		{
			var path = Path.Combine(AppContext.BaseDirectory, $"{arg2.Name}.dll");
			if (File.Exists(path))
			{
				return arg1.LoadFromAssemblyPath(path);
			}

			return null;
		}

		private static List<string> DetectDlls()
		{
#if NETCOREAPP1_1
			var path = Path.Combine(AppContext.BaseDirectory);
#else
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
#endif
			//return new List<string> { Path.Combine(path, "DotnetSpider.Core.dll") };
			return Directory.GetFiles(path).Where(f => f.ToLower().EndsWith("dotnetspider.sample.exe") || f.ToLower().EndsWith("dotnetspider.sample.dll") || f.ToLower().EndsWith("Spiders.dll") || f.ToLower().EndsWith("Spiders.exe")).ToList();
		}
	}
}