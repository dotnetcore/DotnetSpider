using System;
using System.Collections.Generic;
using System.IO;
#if NET_CORE
//using Microsoft.Extensions.Configuration.Ini;
#endif

namespace DotnetSpider.Core.Common
{
	public class ConfigurationManager
	{
		private static readonly Dictionary<string, string> Values = new Dictionary<string, string>();

		//#if NET_CORE
		//		private static IniConfigurationProvider provider;
		//#else
		//		private static readonly Dictionary<string, string> Values = new Dictionary<string, string>();
		//#endif

		static ConfigurationManager()
		{
#if NET_CORE
			string configPath = Path.Combine(AppContext.BaseDirectory, "config.ini");

			//if (File.Exists(configPath))
			//{
			//	provider = new IniConfigurationProvider(new IniConfigurationSource
			//	{
			//		Path = configPath
			//	});
			//	provider.Load();
			//}
#else
			string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
#endif
			if (File.Exists(configPath))
			{
				string[] lines = File.ReadAllLines(configPath);
				foreach (var line in lines)
				{
					if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(line.Trim()) || line.StartsWith("#"))
					{
						continue;
					}
					int firstSplitIndex = line.IndexOf('=');
					string key = line.Substring(0, firstSplitIndex);
					string value = line.Substring(firstSplitIndex + 1, line.Length - firstSplitIndex - 1);
					if (value.Contains(key))
					{
						throw new ArgumentException($"There is a same key already: {key}");
					}
					else
					{
						Values.Add(key, value);
					}
				}
			}

		}

		public static string Get(string key)
		{
//#if NET_CORE
//			string value;
//			provider.TryGet(key, out value);
//			return value;
//#else
//		return Values.ContainsKey(key) ? Values[key] : null;
//#endif
			return Values.ContainsKey(key) ? Values[key] : null;
		}
	}
}
