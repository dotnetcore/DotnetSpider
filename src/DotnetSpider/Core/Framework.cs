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
using System.Threading.Tasks;
using DotnetSpider.Network;
using Microsoft.Extensions.Configuration;

namespace DotnetSpider.Core
{
	public static class Framework
	{
		private static readonly Dictionary<string, string> SwitchMappings =
			new Dictionary<string, string>
			{
				{"-s", "spider"},
				{"-n", "name"},
				{"-i", "id"},
				{"-a", "args"},
				{"-d", "distribute"},
				{"-c", "config"}
			};

		private const string DefaultAppsettings = "appsettings.json";

		public const string ResponseHandlerTopic = "ResponseHandler-";
		public const string DownloaderCenterTopic = "DownloadCenter";
		public const string StatisticsServiceTopic = "StatisticsService";

		public const string AllocateDownloaderCommand = "Allocate";
		public const string DownloadCommand = "Download";
		public const string RegisterCommand = "Register";
		public const string HeartbeatCommand = "Heartbeat";
		public const string ExitCommand = "Exit";
		public const string CommandSeparator = "|";

		public static string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

		public static string GlobalDirectory =
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dotnetspider");

		public static string ChromeDriverPath;
		public static readonly bool IsServer2008;
		public static readonly int TotalMemory;
		public static readonly string IpAddress;
		public static readonly string OsDescription;
		public static WebProxy FiddlerProxy = new WebProxy("http://127.0.0.1:8888");
		public static Task CompletedTask = Task.FromResult(0);

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
			else
			{
				// TODO:
			}

			var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
				.First(i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
			var unicastAddresses = networkInterface.GetIPProperties().UnicastAddresses;
			IpAddress = unicastAddresses.First(a =>
					a.IPv4Mask.ToString() != "255.255.255.255" && a.Address.AddressFamily == AddressFamily.InterNetwork)
				.Address.ToString();

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
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				var lines = File.ReadAllLines("/proc/meminfo");
				var infoDic = lines
					.Select(line => line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Take(2).ToList())
					.ToDictionary(items => items[0], items => long.Parse(items[1]));
				var free = infoDic["MemFree:"];
				var sReclaimable = infoDic["SReclaimable:"];
				return (free + sReclaimable) / 1024;
			}
			else
			{
				return 0;
			}
		}

		public static void SetEncoding()
		{
#if NETSTANDARD
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
		}

		public static void SetMultiThread()
		{
			ThreadPool.SetMinThreads(256, 256);
#if !NETSTANDARD
            ServicePointManager.DefaultConnectionLimit = 1000;
#endif
		}

		public static ConfigurationBuilder CreateConfigurationBuilder(string config = null, string[] args = null,
			bool loadCommandLine = true)
		{
			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
			configurationBuilder.AddEnvironmentVariables();

			if (loadCommandLine)
			{
				configurationBuilder.AddCommandLine(Environment.GetCommandLineArgs(), SwitchMappings);
			}

			if (args != null)
			{
				configurationBuilder.AddCommandLine(args, SwitchMappings);
			}

			if (!string.IsNullOrWhiteSpace(config))
			{
				configurationBuilder.AddJsonFile(config, false,
					true);
			}
			else
			{
				if (File.Exists(DefaultAppsettings))
				{
					configurationBuilder.AddJsonFile(DefaultAppsettings, false,
						true);
				}
			}

			return configurationBuilder;
		}

		public static IConfiguration CreateConfiguration(string config = null, string[] args = null)
		{
			return CreateConfigurationBuilder(config, args).Build();
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
				Console.WriteLine("== Version: 4.0.0                                               ==");
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

			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < width; ++i)
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