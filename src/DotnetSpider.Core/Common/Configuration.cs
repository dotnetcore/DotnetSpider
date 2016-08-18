using System.IO;
#if !NET_CORE
using System;
using System.Collections.Generic;
#else
using Microsoft.Extensions.Configuration;
#endif

namespace DotnetSpider.Core.Common
{
	public class Configuration
	{
#if NET_CORE
		private static readonly IConfigurationRoot ConfigurationRoot;
#else
		private static readonly Dictionary<string, string> Values = new Dictionary<string, string>();
#endif

		static Configuration()
		{
			string configPath = Path.Combine(SpiderEnviroment.BaseDirectory, "config.ini");

			if (File.Exists(configPath))
			{
#if NET_CORE
				IConfigurationBuilder builder = new ConfigurationBuilder();
				builder.AddIniFile("config.ini");
				ConfigurationRoot = builder.Build();
#else
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
					if (value.StartsWith("\"") && value.EndsWith("\""))
					{
						value = value.Substring(1, value.Length - 2);
					}
					if (Values.ContainsKey(key))
					{
						throw new ArgumentException($"There is a same key already: {key}");
					}
					else
					{
						Values.Add(key, value);
					}
				}
#endif
			}
		}

		public static string GetValue(string key)
		{
#if NET_CORE
			return ConfigurationRoot?.GetValue<string>(key);
#else
			return Values.ContainsKey(key) ? Values[key] : null;
#endif
		}
	}
}
