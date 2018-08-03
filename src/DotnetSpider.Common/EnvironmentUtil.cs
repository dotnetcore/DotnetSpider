using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace DotnetSpider.Common
{
	public static class EnvironmentUtil
	{
		public static readonly bool IsWindows;
		public static readonly bool IsServer2008;
		public static readonly int TotalMemory;
		public static readonly string IpAddress;
		public static readonly string OSDescription;

		static EnvironmentUtil()
		{
#if NETFRAMEWORK
			IsWindows = true;
#else
			IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
			var systemVersioin = Environment.OSVersion.Version.ToString();
			IsServer2008 = systemVersioin.StartsWith("6.0") || systemVersioin.StartsWith("6.1");

			if (IsWindows)
			{
				Memorystatus mStatus = new Memorystatus();
				GlobalMemoryStatus(ref mStatus);
				TotalMemory = (int)(Convert.ToInt64(mStatus.DwTotalPhys) / 1024 / 1024);
			}
			else
			{
				var lines = File.ReadAllLines("/proc/meminfo");
				var infoDic = new Dictionary<string, long>();
				foreach (var line in lines)
				{
					var datas = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Take(2).ToList();
					infoDic.Add(datas[0], long.Parse(datas[1]));
				}
				TotalMemory = (int)(infoDic["MemTotal:"] / 1024);
			}

			var interf = NetworkInterface.GetAllNetworkInterfaces().First(i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
			var unicastAddresses = interf.GetIPProperties().UnicastAddresses;
			IpAddress = unicastAddresses.First(a => a.IPv4Mask.ToString() != "255.255.255.255" && a.Address.AddressFamily == AddressFamily.InterNetwork).Address.ToString();

			OSDescription = $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}";
		}

		public static decimal GetCpuLoad()
		{
			decimal total = 100;

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
				string info = process.StandardOutput.ReadToEnd();
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
				Memorystatus mStatus = new Memorystatus();
				GlobalMemoryStatus(ref mStatus);
				return Convert.ToInt64(mStatus.DwAvailPhys) / 1024 / 1024;
			}
			else
			{
				var lines = File.ReadAllLines("/proc/meminfo");
				var infoDic = new Dictionary<string, long>();
				foreach (var line in lines)
				{
					var datas = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Take(2).ToList();
					infoDic.Add(datas[0], long.Parse(datas[1]));
				}
				var free = infoDic["MemFree:"];
				var sReclaimable = infoDic["SReclaimable:"];
				return (free + sReclaimable) / 1024;
			}
		}

		private struct Memorystatus
		{
			public uint DwLength { get; set; }
			public uint DwMemoryLoad { get; set; }
			public UInt64 DwTotalPhys { get; set; } //总的物理内存大小
			public UInt64 DwAvailPhys { get; set; } //可用的物理内存大小 
			public UInt64 DwTotalPageFile { get; set; }
			public UInt64 DwAvailPageFile { get; set; } //可用的页面文件大小
			public UInt64 DwTotalVirtual { get; set; } //返回调用进程的用户模式部分的全部可用虚拟地址空间
			public UInt64 DwAvailVirtual { get; set; } // 返回调用进程的用户模式部分的实际自由可用的虚拟地址空间
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GlobalMemoryStatus(ref Memorystatus lpBuffer);
	}
}
