using System;
using System.Linq;
using System.Reflection;

namespace DotnetSpider.Test
{
	public class TestManager
	{
		public static void Run()
		{
			var types = typeof(TestManager).GetTypeInfo().Assembly.GetTypes();
			foreach (var type in types)
			{

				Console.WriteLine($"Fetch: {type.FullName} ...");

				var allMethods = type.GetTypeInfo().DeclaredMethods;
				var methodInfos = allMethods as MethodInfo[] ?? allMethods.ToArray();
				var methods = methodInfos.Where(m => m.GetCustomAttribute<TestMethod>() != null);

				var initMethod = methodInfos.FirstOrDefault(m => m.GetCustomAttribute<TestInitialize>() != null);
				foreach (var methodInfo in methods)
				{
					var obj = Activator.CreateInstance(type);
					try
					{
						initMethod?.Invoke(obj, null);
						methodInfo.Invoke(obj, null);
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine($"[{type.Name}] [{methodInfo.Name}] Pass.");
					}
					catch (Exception e)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"[{type.Name}] [{methodInfo.Name}] Faild: {e}");
					}
				}
			}

		}
	}
}
