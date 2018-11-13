using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DotnetSpider.Core.Infrastructure
{
	public static class EnvironmentUtil
	{
		public static readonly bool IsWindows;
		public static readonly bool IsServer2008;
		public static readonly int TotalMemory;
		public static readonly string IpAddress;
		public static readonly string OsDescription;

		static EnvironmentUtil()
		{
#if NETFRAMEWORK
			IsWindows = true;
#else
			IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
			var systemVersion = Environment.OSVersion.Version.ToString();
			IsServer2008 = systemVersion.StartsWith("6.0") || systemVersion.StartsWith("6.1");

			if (IsWindows)
			{
				var mStatus = new MemoryStatus();
				GlobalMemoryStatus(ref mStatus);
				TotalMemory = (int)(Convert.ToInt64(mStatus.DwTotalPhys) / 1024 / 1024);
			}
			else
			{
				var lines = File.ReadAllLines("/proc/meminfo");
				var infoDic = lines.Select(line => line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Take(2).ToList()).ToDictionary(items => items[0], items => long.Parse(items[1]));
				TotalMemory = (int)(infoDic["MemTotal:"] / 1024);
			}

			var networkInterface = NetworkInterface.GetAllNetworkInterfaces().First(i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
			var unicastAddresses = networkInterface.GetIPProperties().UnicastAddresses;
			IpAddress = unicastAddresses.First(a => a.IPv4Mask.ToString() != "255.255.255.255" && a.Address.AddressFamily == AddressFamily.InterNetwork).Address.ToString();

			OsDescription = $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}";
		}

		public static decimal GetCpuLoad()
		{
			decimal total;

			if (IsWindows)
			{
				var process = new System.Diagnostics.Process
				{
					StartInfo =
						{
							FileName = "wmic",
							Arguments = "cpu get LoadPercentage",
							CreateNoWindow = false,
							RedirectStandardOutput = true,
							RedirectStandardInput = true,
							UseShellExecute=false
						}
				};
				process.Start();
				var info = process.StandardOutput.ReadToEnd();
				var lines = info.Split('\n');
				process.WaitForExit();
				process.Dispose();
				if (lines.Length > 1)
				{
					var loadStr = lines[1].Trim();
					total = decimal.Parse(loadStr);
				}
				else
				{
					total = 99;
				}
			}
			else
			{
				total = LinuxCpuLoad.Get();
			}

			return total;
		}

		public static long GetFreeMemory()
		{
			if (IsWindows)
			{
				var mStatus = new MemoryStatus();
				GlobalMemoryStatus(ref mStatus);
				return Convert.ToInt64(mStatus.DwAvailPhys) / 1024 / 1024;
			}
			else
			{
				var lines = File.ReadAllLines("/proc/meminfo");
				var infoDic = lines.Select(line => line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Take(2).ToList()).ToDictionary(items => items[0], items => long.Parse(items[1]));
				var free = infoDic["MemFree:"];
				var sReclaimable = infoDic["SReclaimable:"];
				return (free + sReclaimable) / 1024;
			}
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
