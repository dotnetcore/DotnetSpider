using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DotnetSpider.Infrastructure
{
	public static class SystemInformation
	{
		private static readonly Stopwatch _getMemoryStatusWatch = new();
		private static MemoryInfo _memoryInfo;

		public static readonly int TotalMemory;

		static SystemInformation()
		{
			var mi = GetMemoryInfo();
			TotalMemory = (int)(mi.UllTotalPhys / 1024 / 1024);
		}

		public static int FreeMemory => (int)(GetMemoryInfo().UllAvailPhys / 1024 / 1024);

		public static int UsedMemory
		{
			get
			{
				var mi = GetMemoryInfo();
				return (int)((mi.UllTotalPhys - mi.UllAvailPhys) / 1024 / 1024);
			}
		}

		private static unsafe MemoryInfo GetMemoryInfo()
		{
			lock (_getMemoryStatusWatch)
			{
				if (!_getMemoryStatusWatch.IsRunning || _getMemoryStatusWatch.ElapsedMilliseconds >= 500)
				{
					_getMemoryStatusWatch.Restart();
					_memoryInfo.DwLength = (uint)sizeof(MemoryInfo);
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					{
						GlobalMemoryStatusEx(ref _memoryInfo);
					}
					else
					{
						GetLinuxMemoryInfo(ref _memoryInfo);
					}
				}

				return _memoryInfo;
			}
		}

		private static void GetLinuxMemoryInfo(ref MemoryInfo mi)
		{
			var path = "/proc/meminfo";
			if (!File.Exists(path))
			{
				return;
			}

			FileStream stat;
			try
			{
				stat = File.OpenRead(path);
			}
			catch (Exception)
			{
				return;
			}

			long? call(string line, string key)
			{
				var i = line.IndexOf(':');
				if (i < 0)
				{
					return null;
				}

				var lk = line.Substring(0, i);
				if (string.IsNullOrEmpty(lk))
				{
					return null;
				}

				if (lk != key)
				{
					return null;
				}

				line = line.Substring(i + 1).TrimStart();
				if (string.IsNullOrEmpty(line))
				{
					return null;
				}

				var sp = line.Split(' ');
				if (sp.Length <= 0)
				{
					return null;
				}

				line = sp[0];
				if (string.IsNullOrEmpty(line))
				{
					return null;
				}

				long.TryParse(line, out var n);
				return n * 1024;
			}

			using var sr = new StreamReader(stat);
			try
			{
				var counts = 0;
				string line;
				while (counts < 2 && !string.IsNullOrEmpty(line = sr.ReadLine()))
				{
					var value = call(line, "MemTotal");
					if (value != null)
					{
						counts++;
						mi.UllTotalPhys = value.Value;
						continue;
					}

					value = call(line, "MemAvailable");
					if (value != null)
					{
						counts++;
						mi.UllAvailPhys = value.Value;
					}
				}
			}
			catch (Exception)
			{
				//ignore
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct MemoryInfo
		{
			public uint DwLength; // 当前结构体大小
			public uint DwMemoryLoad; // 当前内存使用率
			public long UllTotalPhys; // 总计物理内存大小
			public long UllAvailPhys; // 可用物理内存大小
			public long UllTotalPageFile; // 总计交换文件大小
			public long UllAvailPageFile; // 总计交换文件大小
			public long UllTotalVirtual; // 总计虚拟内存大小
			public long UllAvailVirtual; // 可用虚拟内存大小
			public long UllAvailExtendedVirtual; // 保留 这个值始终为0
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GlobalMemoryStatusEx(ref MemoryInfo mi);
	}
}
