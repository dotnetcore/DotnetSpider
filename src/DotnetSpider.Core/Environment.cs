#if NET_CORE
using System.Runtime.InteropServices;
#endif
using System;
using System.Configuration;
using System.IO;

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

		public static ConnectionStringSettings SystemConnectionStringSettings { get; set; }
		public static ConnectionStringSettings DataConnectionStringSettings { get; set; }
		public static string SystemConnectionString => SystemConnectionStringSettings?.ConnectionString;
		public static string DataConnectionString => DataConnectionStringSettings?.ConnectionString;

		public static string GetAppSettings(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}

		public static ConnectionStringSettings GetConnectStringSettings(string key)
		{
			return ConfigurationManager.ConnectionStrings[key];
		}

		static Environment()
		{
			RedisConnectString = ConfigurationManager.AppSettings[RedisConnectStringKey]?.Trim();
			EmailHost = ConfigurationManager.AppSettings[EmailHostKey]?.Trim();
			EmailPort = ConfigurationManager.AppSettings[EmailPortKey]?.Trim();
			EmailAccount = ConfigurationManager.AppSettings[EmailAccountKey]?.Trim();
			EmailPassword = ConfigurationManager.AppSettings[EmailPasswordKey]?.Trim();
			EmailDisplayName = ConfigurationManager.AppSettings[EmailDisplayNameKey]?.Trim();

			SystemConnectionStringSettings = ConfigurationManager.ConnectionStrings[SystemConnectionStringKey];
			DataConnectionStringSettings = ConfigurationManager.ConnectionStrings[DataConnectionStringKey];

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
