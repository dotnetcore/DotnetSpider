using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using DotnetSpider.Network;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Common
{
	public static class Framework
	{
		public static Dictionary<string, string> SwitchMappings =
			new Dictionary<string, string>
			{
				{"-t", "TYPE"},
				{"-n", "NAME"},
				{"-i", "ID"},
				{"-c", "CONFIG"},
				{"-d", "DISTRIBUTED"}
			};

		private const string DefaultAppSettings = "appsettings.json";

		public const string DownloadCommand = "Download";
		public const string RegisterCommand = "Register";
		public const string HeartbeatCommand = "Heartbeat";
		public const string ExitCommand = "Exit";
		public const string CommandSeparator = "|";

		public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

		public static readonly string GlobalDirectory =
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dotnetspider");

		public static string ChromeDriverPath;
		public static readonly bool IsServer2008;
		public static readonly int TotalMemory;
		public static readonly string IpAddress;
		public static readonly string OsDescription;
		public static WebProxy FiddlerProxy = new WebProxy("http://127.0.0.1:8888");

		public static NetworkCenter NetworkCenter;

		static Framework()
		{
			var systemVersion = Environment.OSVersion.Version.ToString();
			IsServer2008 = systemVersion.StartsWith("6.0") || systemVersion.StartsWith("6.1");

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				var mStatus = new MemoryStatus();
				GlobalMemoryStatus(ref mStatus);
				TotalMemory = (int) (Convert.ToInt64(mStatus.DwTotalPhys) / 1024 / 1024);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				var lines = File.ReadAllLines("/proc/meminfo");
				var infoDic = lines
					.Select(line => line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Take(2).ToList())
					.ToDictionary(items => items[0], items => long.Parse(items[1]));
				TotalMemory = (int) (infoDic["MemTotal:"] / 1024);
			}

			IpAddress = NetworkHelper.CurrentIpAddress();

			OsDescription = $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}";
		}

		public static long GetFreeMemory()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				var mStatus = new MemoryStatus();
				GlobalMemoryStatus(ref mStatus);
				return Convert.ToInt64(mStatus.DwAvailPhys) / 1024 / 1024;
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				var lines = File.ReadAllLines("/proc/meminfo");
				var infoDic = lines
					.Select(line => line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Take(2).ToList())
					.ToDictionary(items => items[0], items => long.Parse(items[1]));
				var free = infoDic["MemFree:"];
				var sReclaimable = infoDic["SReclaimable:"];
				return (free + sReclaimable) / 1024;
			}

			return 0;
		}

		public static void RegisterEncoding()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		public static void SetMultiThread()
		{
			ThreadPool.SetMinThreads(256, 256);
#if !NETSTANDARD
            ServicePointManager.DefaultConnectionLimit = 1000;
#endif
		}

		public static ConfigurationBuilder CreateConfigurationBuilder(string config = null)
		{
			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);

			if (File.Exists(DefaultAppSettings))
			{
				configurationBuilder.AddJsonFile(DefaultAppSettings, false,
					true);
			}

			if (!string.IsNullOrWhiteSpace(config) && config != DefaultAppSettings && File.Exists(config))
			{
				configurationBuilder.AddJsonFile(config, false,
					true);
			}
			configurationBuilder.AddCommandLine(Environment.GetCommandLineArgs(), SwitchMappings);
			configurationBuilder.AddEnvironmentVariables();
			return configurationBuilder;
		}

		public static IConfiguration CreateConfiguration(string config = null, string[] args = null)
		{
			return CreateConfigurationBuilder(config).Build();
		}

		/// <summary>
		/// 打印爬虫框架信息
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void PrintInfo()
		{
			var key = "PRINT_DOTNET_SPIDER_INFO";

			var isPrinted = AppDomain.CurrentDomain.GetData(key) != null;

			if (!isPrinted)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("==================================================================");
				Console.WriteLine("== DotnetSpider is an open source crawler developed by C#       ==");
				Console.WriteLine("== It's multi thread, light weight, stable and high performance ==");
				Console.WriteLine("== Support storage data to file, mysql, mssql, mongodb etc      ==");
				Console.WriteLine("== License: MIT                                                 ==");
				Console.WriteLine("== Author: zlzforever@163.com                                   ==");
				Console.WriteLine("==================================================================");
				Console.ForegroundColor = ConsoleColor.White;

				AppDomain.CurrentDomain.SetData(key, "True");
			}
		}

		/// <summary>
		/// 打印一整行word到控制台中
		/// </summary>
		/// <param name="word">打印的字符</param>
		public static void PrintLine(char word = '=')
		{
			var width = 120;

			try
			{
				width = Console.WindowWidth;
			}
			catch
			{
				// ignore
			}

			var builder = new StringBuilder();
			for (var i = 0; i < width; ++i)
			{
				builder.Append(word);
			}

			Console.Write(builder.ToString());
		}

		private struct MemoryStatus
		{
			public uint DwLength { get; set; }
			public uint DwMemoryLoad { get; set; }
			public ulong DwTotalPhys { get; set; } //总的物理内存大小
			public ulong DwAvailPhys { get; set; } //可用的物理内存大小 
			public ulong DwTotalPageFile { get; set; }
			public ulong DwAvailPageFile { get; set; } //可用的页面文件大小
			public ulong DwTotalVirtual { get; set; } //返回调用进程的用户模式部分的全部可用虚拟地址空间
			public ulong DwAvailVirtual { get; set; } // 返回调用进程的用户模式部分的实际自由可用的虚拟地址空间
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GlobalMemoryStatus(ref MemoryStatus lpBuffer);
	}
}