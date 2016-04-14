using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Java2Dotnet.Spider.Common
{
	public class ConfigurationManager
	{
		static readonly Dictionary<string, string> Values = new Dictionary<string, string>();

		static ConfigurationManager()
		{
#if NET_CORE
			string configPath= Path.Combine(AppContext.BaseDirectory,"app.conf");
#else
			string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.conf");
#endif
			if (File.Exists(configPath))
			{
				string[] lines = File.ReadAllLines(configPath);
				foreach (var line in lines)
				{
					int firstSplitIndex = line.IndexOf(':');
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
			return Values.ContainsKey(key) ? Values[key] : null;
		}
	}
}
