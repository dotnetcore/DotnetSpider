#if NET_CORE
using System.Runtime.InteropServices;
#endif
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Linq;

namespace DotnetSpider.Core
{
	//
	// Summary:
	//     Provides information about, and means to manipulate, the current environment
	//     and platform. This class cannot be inherited.
	public static class Env
	{
		public const string Version = "2.0.21";
		public const string RedisConnectStringKey = "redisConnectString";
		public const string EmailHostKey = "emailHost";
		public const string EmailPortKey = "emailPort";
		public const string EmailAccountKey = "emailAccount";
		public const string EmailPasswordKey = "emailPassword";
		public const string EmailDisplayNameKey = "emailDisplayName";
		public const string SystemConnectionStringKey = "SystemConnection";
		public const string DataConnectionStringKey = "DataConnection";
		public static readonly string[] IdColumns = new[] { "Id", "__Id" };
		public const string EnvLocation = "LOCATION";
		public const string EnvConfig = "CONFIG";
		public const string EnvDbConfig = "DBCONFIG";
		public const string CDateColumn = "CDate";

		public static ConnectionStringSettings SystemConnectionStringSettings { get; private set; }
		public static ConnectionStringSettings DataConnectionStringSettings { get; private set; }

		public static string HostName { get; set; }
		public static string Ip { get; set; }
		public static string RedisConnectString { get; private set; }
		public static string EmailHost { get; private set; }
		public static string EmailPort { get; private set; }
		public static string EmailAccount { get; private set; }
		public static string EmailPassword { get; private set; }
		public static string EmailDisplayName { get; private set; }
		public static bool SaveLogAndStatusToDb => SystemConnectionStringSettings != null;
		public static string GlobalDirectory { get; private set; }
		public static string BaseDirectory { get; private set; }
		public static string PathSeperator { get; private set; }

		public static string SystemConnectionString => SystemConnectionStringSettings?.ConnectionString;
		public static string DataConnectionString => DataConnectionStringSettings?.ConnectionString;

		public static string GlobalAppConfigPath;

		public static Configuration GlobalConfiguraiton;

		public static string GetAppSettings(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}

		public static ConnectionStringSettings GetConnectStringSettings(string key)
		{
			return ConfigurationManager.ConnectionStrings[key];
		}

		public static void LoadConfiguration(string filePath)
		{
			var path = File.Exists(filePath) ? filePath : Path.Combine(BaseDirectory, "app.config");
			var fileMap = new ExeConfigurationFileMap
			{
				ExeConfigFilename = path
			};

			var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

			RedisConnectString = configuration.AppSettings.Settings[RedisConnectStringKey]?.Value?.Trim();
			EmailHost = configuration.AppSettings.Settings[EmailHostKey]?.Value?.Trim();
			EmailPort = configuration.AppSettings.Settings[EmailPortKey]?.Value?.Trim();
			EmailAccount = configuration.AppSettings.Settings[EmailAccountKey]?.Value?.Trim();
			EmailPassword = configuration.AppSettings.Settings[EmailPasswordKey]?.Value?.Trim();
			EmailDisplayName = configuration.AppSettings.Settings[EmailDisplayNameKey]?.Value?.Trim();

			if ("GLOBAL" == AppDomain.CurrentDomain.GetData(EnvDbConfig)?.ToString().ToUpper())
			{
				if (File.Exists(GlobalAppConfigPath))
				{
					var globalFileMap = new ExeConfigurationFileMap
					{
						ExeConfigFilename = GlobalAppConfigPath
					};

					GlobalConfiguraiton =
						ConfigurationManager.OpenMappedExeConfiguration(globalFileMap, ConfigurationUserLevel.None);

					SystemConnectionStringSettings =
						GlobalConfiguraiton.ConnectionStrings.ConnectionStrings[SystemConnectionStringKey];
					DataConnectionStringSettings =
						GlobalConfiguraiton.ConnectionStrings.ConnectionStrings[DataConnectionStringKey];
				}
				else
				{
					throw new SpiderException("Global app.config unfound.");
				}
			}
			else
			{
				SystemConnectionStringSettings =
					configuration.ConnectionStrings.ConnectionStrings[SystemConnectionStringKey];
				DataConnectionStringSettings =
					configuration.ConnectionStrings.ConnectionStrings[DataConnectionStringKey];
			}
		}

		static Env()
		{
			Reload();
		}

		public static void Reload()
		{
			BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

#if !NET_CORE
			PathSeperator = "\\";
#else
			PathSeperator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/";
#endif

			GlobalDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dotnetspider");

			DirectoryInfo di = new DirectoryInfo(GlobalDirectory);
			if (!di.Exists)
			{
				di.Create();
			}

			GlobalAppConfigPath = Path.Combine(GlobalDirectory, "app.config");

			HostName = Dns.GetHostName();
			var addresses = Dns.GetHostAddresses(HostName);
			Ip = addresses.FirstOrDefault(i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
				?.ToString();

			var appConfigName = AppDomain.CurrentDomain.GetData(EnvConfig)?.ToString();

			var path = string.IsNullOrEmpty(appConfigName)
				? Path.Combine(BaseDirectory, "app.config")
				: Path.Combine(BaseDirectory, appConfigName);

			LoadConfiguration(path);
		}
	}
}