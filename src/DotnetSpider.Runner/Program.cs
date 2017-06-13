using DotnetSpider.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotnetSpider.Runner
{
	public class Program
	{
		public static void Main(string[] args)
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
			var asl = new AssemblyLoader(AppContext.BaseDirectory);
#endif
			int totalTypesCount = 0;
			var spiders = new Dictionary<string, IRunable>();
			foreach (var spiderDll in DetectDlls())
			{
#if NETCOREAPP1_1
				var asm = asl.LoadFromAssemblyPath(Path.Combine(AppContext.BaseDirectory, "netcore", spiderDll));
				var types = asm.GetTypes();
#else

				var asm = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "net45", spiderDll));
				var types = asm.GetTypes();
#endif

				foreach (var type in types)
				{
					bool hasNonParametersConstructor = type.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Length == 0);

					if (hasNonParametersConstructor)
					{
						dynamic obj = Activator.CreateInstance(type);
						var named = obj as INamed;
						var runner = obj as IRunable;

						if (named != null && runner != null)
						{
							if (!spiders.ContainsKey(named.Name))
							{
								spiders.Add(named.Name, runner);
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
				var batch = spiders[spiderName] as IBatch;
				batch.Batch = arguments["-b"];
			}
			if (arguments.ContainsKey("-n"))
			{
				var named = spiders[spiderName] as INamed;
				named.Name = arguments["-n"];
			}

			if (!arguments.ContainsKey("-a"))
			{
				spiders[spiderName].Run();
			}
			else
			{
				spiders[spiderName].Run(arguments["-a"]);
			}
		}

		private static List<string> DetectDlls()
		{
#if NETCOREAPP1_1
			var path = Path.Combine(AppContext.BaseDirectory, "Spiders","netcore");
#else
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Spiders", "net45");
#endif
			return Directory.GetFiles(path).Where(f => f.ToLower().EndsWith(".dll") || f.ToLower().EndsWith(".exe")).ToList();
		}
	}
}