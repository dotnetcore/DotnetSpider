#if NET_CORE
using System.Runtime.InteropServices;
#endif
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

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
		public const string HttpCenterKey = "serviceUrl";
		public const string HttpCenterTokenKey = "serviceToken";
		public const string SystemConnectionStringKey = "SystemConnection";
		public const string DataConnectionStringKey = "DataConnection";
		public static readonly string[] IdColumns = { "Id", "__Id" };
		public const string CDateColumn = "CDate";
		public const string SqlEncryptCodeKey = "sqlEncryptCode";

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
		public static string GlobalDirectory { get; private set; }
		internal static string DefaultGlobalAppConfigPath { get; private set; }
		public static string BaseDirectory { get; private set; }
		public static string PathSeperator { get; private set; }
		public static bool SubmitHttpLog { get; set; }
		public static string HttpCenter { get; private set; }
		public static string HttpLogUrl { get; private set; }
		public static string HttpPipelineUrl { get; private set; }
		public static string HttpStatusUrl { get; private set; }
		public static string HttpIncreaseRunningUrl { get; private set; }
		public static string HttpReduceRunningUrl { get; private set; }
		public static string HttpCenterToken { get; private set; }
		public static string SystemConnectionString => SystemConnectionStringSettings?.ConnectionString;
		public static string DataConnectionString => DataConnectionStringSettings?.ConnectionString;
		public static bool ProcessorFilterDefaultRequest = true;
		public static string SqlEncryptCode { get; private set; }
		public static int IdentityMaxLength { get; set; } = 120;

		public static string GetAppSettings(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}

		public static ConnectionStringSettings GetConnectStringSettings(string key)
		{
			return ConfigurationManager.ConnectionStrings[key];
		}

		public static void LoadConfiguration(string path)
		{
			var globalStr = "%global%";

			if (path.ToLower().StartsWith(globalStr))
			{
				var fileName = path.Substring(8, path.Length - 8);
				path = string.IsNullOrEmpty(fileName) ? Path.Combine(GlobalDirectory, "app.config") : Path.Combine(GlobalDirectory, fileName);
			}

			if (string.IsNullOrEmpty(path))
			{
				path = Path.Combine(BaseDirectory, "app.config");
			}
			if (!File.Exists(path))
			{
				return;
			}

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
			HttpCenter = configuration.AppSettings.Settings[HttpCenterKey]?.Value?.Trim();
			if (!string.IsNullOrEmpty(HttpCenter))
			{
				HttpLogUrl = $"{HttpCenter}{(HttpCenter.EndsWith("/") ? "" : "/")}Log/submit";
				HttpCenterToken = configuration.AppSettings.Settings[HttpCenterTokenKey]?.Value?.Trim();
				HttpStatusUrl = $"{HttpCenter}{(HttpCenter.EndsWith("/") ? "" : "/")}TaskStatus/AddOrUpdate";
				HttpIncreaseRunningUrl = $"{HttpCenter}{(HttpCenter.EndsWith("/") ? "" : "/")}Task/IncreaseRunning";
				HttpReduceRunningUrl = $"{HttpCenter}{(HttpCenter.EndsWith("/") ? "" : "/")}Task/ReduceRunning";
				HttpPipelineUrl = $"{HttpCenter}{(HttpCenter.EndsWith("/") ? "" : "/")}Pipeline/Process";
			}
			SubmitHttpLog = !string.IsNullOrEmpty(HttpLogUrl);
			SystemConnectionStringSettings = configuration.ConnectionStrings.ConnectionStrings[SystemConnectionStringKey];
			DataConnectionStringSettings = configuration.ConnectionStrings.ConnectionStrings[DataConnectionStringKey];
			SqlEncryptCode = configuration.AppSettings.Settings[SqlEncryptCodeKey]?.Value?.Trim();
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
			DefaultGlobalAppConfigPath = Path.Combine(GlobalDirectory, "app.config");
			DirectoryInfo di = new DirectoryInfo(GlobalDirectory);
			if (!di.Exists)
			{
				di.Create();
			}

			HostName = Dns.GetHostName();

			var interf = NetworkInterface.GetAllNetworkInterfaces().First(i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
			var unicastAddresses = interf.GetIPProperties().UnicastAddresses;
			Ip = unicastAddresses.FirstOrDefault(a => a.IPv4Mask?.ToString() != "255.255.255.255" && a.Address.AddressFamily == AddressFamily.InterNetwork)?.Address.ToString();
			var path = Path.Combine(BaseDirectory, "app.config");

			LoadConfiguration(path);
		}
	}
}
