using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Portal
{
	public class Configuration
	{
		private static readonly IConfigurationRoot ConfigurationRoot;

		static Configuration()
		{
			string configPath = Path.Combine(AppContext.BaseDirectory, "config.ini");

			if (File.Exists(configPath))
			{
				IConfigurationBuilder builder = new ConfigurationBuilder();
				builder.AddIniFile("config.ini");
				ConfigurationRoot = builder.Build();
			}
		}

		public static string GetValue(string key)
		{
			return ConfigurationRoot?.GetValue<string>(key);
		}
	}
}
