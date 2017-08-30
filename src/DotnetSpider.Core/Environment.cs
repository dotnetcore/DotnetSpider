#if NET_CORE
using System.Runtime.InteropServices;
#endif
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace DotnetSpider.Core
{
	public static class Environment
	{
		public const string RedisConnectStringKey = "redisConnectString";
		public const string EmailHostKey = "emailHost";
		public const string EmailPortKey = "emailPort";
		public const string EmailAccountKey = "emailAccount";
		public const string EmailPasswordKey = "emailPassword";
		public const string EmailDisplayNameKey = "emailDisplayName";
		public const string SystemConnectionStringKey = "SystemConnection";
		public const string DataConnectionStringKey = "DataConnection";
		public const string IdColumn = "__id";

		public static readonly Configuration Configuration;
		public static readonly ConnectionStringSettings SystemConnectionStringSettings;
		public static readonly ConnectionStringSettings DataConnectionStringSettings;

		public static string RedisConnectString { get; }
		public static string EmailHost { get; }
		public static string EmailPort { get; }
		public static string EmailAccount { get; }
		public static string EmailPassword { get; }
		public static string EmailDisplayName { get; }
		public static bool SaveLogAndStatusToDb { get; }
		public static string GlobalDirectory { get; }
		public static string BaseDirectory { get; }
		public static string PathSeperator { get; }

		public static string SystemConnectionString => SystemConnectionStringSettings?.ConnectionString;
		public static string DataConnectionString => DataConnectionStringSettings?.ConnectionString;

		public static string GetAppSettings(string key)
		{
			if (Configuration == null)
			{
				return ConfigurationManager.AppSettings[key];
			}
			else
			{
				return Configuration.AppSettings.Settings[key].Value;
			}
		}

		public static ConnectionStringSettings GetConnectStringSettings(string key)
		{
			if (Configuration == null)
			{
				return ConfigurationManager.ConnectionStrings[key];
			}
			else
			{
				return Configuration.ConnectionStrings.ConnectionStrings[key];
			}
		}

		static Environment()
		{
			var configurationPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory).Where(p => p.EndsWith(".dll.config")).ToList();

			if (configurationPaths.Count > 1)
			{
				throw new ArgumentException($"Allow one application config file, please check your runtime folder: {AppDomain.CurrentDomain.BaseDirectory}");
			}
			if (configurationPaths.Count == 1)
			{
				Configuration = ConfigurationManager.OpenExeConfiguration(configurationPaths[0].Replace(".config", ""));

				RedisConnectString = Configuration.AppSettings.Settings[RedisConnectStringKey].Value?.Trim();
				EmailHost = Configuration.AppSettings.Settings[EmailHostKey].Value?.Trim();
				EmailPort = Configuration.AppSettings.Settings[EmailPortKey].Value?.Trim();
				EmailAccount = Configuration.AppSettings.Settings[EmailAccountKey].Value?.Trim();
				EmailPassword = Configuration.AppSettings.Settings[EmailPasswordKey].Value?.Trim();
				EmailDisplayName = Configuration.AppSettings.Settings[EmailDisplayNameKey].Value?.Trim();

				SystemConnectionStringSettings = Configuration.ConnectionStrings.ConnectionStrings[SystemConnectionStringKey];
				DataConnectionStringSettings = Configuration.ConnectionStrings.ConnectionStrings[DataConnectionStringKey];
			}
			else
			{
				RedisConnectString = ConfigurationManager.AppSettings[RedisConnectStringKey]?.Trim();
				EmailHost = ConfigurationManager.AppSettings[EmailHostKey]?.Trim();
				EmailPort = ConfigurationManager.AppSettings[EmailPortKey]?.Trim();
				EmailAccount = ConfigurationManager.AppSettings[EmailAccountKey]?.Trim();
				EmailPassword = ConfigurationManager.AppSettings[EmailPasswordKey]?.Trim();
				EmailDisplayName = ConfigurationManager.AppSettings[EmailDisplayNameKey]?.Trim();

				SystemConnectionStringSettings = ConfigurationManager.ConnectionStrings[SystemConnectionStringKey];
				DataConnectionStringSettings = ConfigurationManager.ConnectionStrings[DataConnectionStringKey];
			}

#if !NET_CORE
			PathSeperator = "\\";
#else
			PathSeperator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/";
#endif

			SaveLogAndStatusToDb = SystemConnectionStringSettings != null;

#if !NET_CORE
			GlobalDirectory = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "DotnetSpider");
			BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
#else
			BaseDirectory = AppContext.BaseDirectory;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				GlobalDirectory = Path.Combine(System.Environment.GetEnvironmentVariable("HOME"), "dotnetspider");
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				GlobalDirectory = Path.Combine(System.Environment.GetEnvironmentVariable("HOME"), "dotnetspider");
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				GlobalDirectory = $"C:\\Users\\{System.Environment.GetEnvironmentVariable("USERNAME")}\\Documents\\DotnetSpider\\";
			}
			else
			{
				throw new ArgumentException("Unknow OS.");
			}

			DirectoryInfo di = new DirectoryInfo(GlobalDirectory);
			if (!di.Exists)
			{
				di.Create();
			}
#endif
		}
	}
}
