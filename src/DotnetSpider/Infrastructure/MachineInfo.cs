using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DotnetSpider.Infrastructure
{
	public class MachineInfo
	{
		public long AvailableMemory { get; private set; }
		public long Memory { get; private set; }

		// ReSharper disable once InconsistentNaming
		private static class OSX
		{
			private static readonly int _structLength = Marshal.SizeOf<VmStatistics>();
			private static readonly int _pageSize;
			private static readonly long _totalMemory;

			static OSX()
			{
				var host = mach_host_self();
				host_page_size(host, ref _pageSize);
				_totalMemory = GetTotalMemory();
			}

			public static MachineInfo GetMemoryStatus()
			{
				var statistics = new VmStatistics();
				var host = mach_host_self();
				var count = _structLength;
				host_statistics64(host, 2, ref statistics, ref count);
				var free = (statistics.Equals(default)
					? 0
					: (((long)statistics.free_count + statistics.inactive_count) * _pageSize));
				return new MachineInfo {Memory = _totalMemory, AvailableMemory = (free)};
			}

			/// <summary>
			/// MB
			/// </summary>
			/// <returns></returns>
			private static long GetTotalMemory()
			{
				return GetNumber("hw.memsize");
			}

			[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
			private static extern int host_page_size(IntPtr host, ref int pageSize);

			[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
			private static extern int sysctlbyname(string name, StringBuilder value, ref IntPtr length, IntPtr newp,
				IntPtr newLen);

			[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
			private static extern int sysctlbyname(string name, ref long value, ref IntPtr length, IntPtr newp,
				IntPtr newLen);

			[DllImport("libc")]
			private static extern IntPtr mach_host_self();

			[DllImport("libc")]
			private static extern IntPtr host_statistics64(IntPtr host, int hostFlavor, ref VmStatistics vmStat,
				ref int count);

			//[StructLayout(LayoutKind.Sequential, Pack = 1)]
			[SuppressMessage("ReSharper", "InconsistentNaming")]
#pragma warning disable 649
			private readonly struct VmStatistics
			{
				public readonly int free_count; /* # 空闲内存页数量，没有被占用的 */
				public readonly int active_count; /* # 活跃内存页数量，正在使用或者最近被使用 */
				public readonly int inactive_count; /* # 非活跃内存页数量，有数据，但是最近没有被使用过，下一个可能就要干掉他 */
				public readonly int wire_count; /* # 系统占用的内存页，不可被换出的 */
				public readonly ulong zero_fill_count; /* # Filled with Zero Page 的页数 */
				public readonly ulong reactivations; /* # 重新激活的页数 inactive to active */
				public readonly ulong pageins; /* # 换入，写入内存 */
				public readonly ulong pageouts; /* # 换出，写入磁盘 */
				public readonly ulong faults; /* # Page fault 次数 */
				public readonly ulong cow_faults; /* # of copy-on-writes */
				public readonly ulong lookups; /* object cache lookups */
				public readonly ulong hits; /* object cache hits */
				public readonly ulong purges; /* # of pages purged */

				public readonly int purgeable_count; /* # of pages purgeable */

				/*
				 * NB: speculative pages are already accounted for in "free_count",
				 * so "speculative_count" is the number of "free" pages that are
				 * used to hold data that was read speculatively from disk but
				 * haven't actually been used by anyone so far.
				 *
				 */
				public readonly int speculative_count; /* # of pages speculative */

				/* added for rev1 */
				public readonly ulong decompressions; /* # of pages decompressed */
				public readonly ulong compressions; /* # of pages compressed */
				public readonly ulong swapins; /* # of pages swapped in (via compression segments) */
				public readonly ulong swapouts; /* # of pages swapped out (via compression segments) */
				public readonly int compressor_page_count; /* # 压缩过个内存 */
				public readonly int throttled_count; /* # of pages throttled */

				public readonly int
					external_page_count; /* # of pages that are file-backed (non-swap) mmap() 映射到磁盘文件的 */

				public readonly int internal_page_count; /* # of pages that are anonymous malloc() 分配的内存 */

				public readonly ulong
					total_uncompressed_pages_in_compressor; /* # of pages (uncompressed) held within the compressor. */
			}

			// internal static string GetString(string param)
			// {
			// 	var size = IntPtr.Zero;
			// 	sysctlbyname(param, null, ref size, IntPtr.Zero, (IntPtr)0);
			//
			// 	if (size == IntPtr.Zero)
			// 	{
			// 		return null;
			// 	}
			//
			// 	var sb = new StringBuilder();
			//
			// 	sysctlbyname(param, sb, ref size, IntPtr.Zero, (IntPtr)0);
			// 	return sb.ToString();
			// }

			private static long GetNumber(string param)
			{
				var size = IntPtr.Zero;
				sysctlbyname(param, null, ref size, IntPtr.Zero, (IntPtr)0);

				if (size == IntPtr.Zero)
				{
					return 0;
				}

				long sb = 0;
				sysctlbyname(param, ref sb, ref size, IntPtr.Zero, (IntPtr)0);
				return sb;
			}
		}

		private static class Windows
		{
			private static readonly uint _structLength = checked((uint)Marshal.SizeOf<WindowsMemoryStatus>());
			private static readonly long _totalMemory;

			static Windows()
			{
				_totalMemory = GetTotalMemory();
			}

			public static MachineInfo GetMemoryStatus()
			{
				var memoryInfo = new WindowsMemoryStatus {DwLength = _structLength};
				GlobalMemoryStatusEx(ref memoryInfo);
				return new MachineInfo
				{
					Memory = _totalMemory,
					AvailableMemory = memoryInfo.Equals(default) ? 0 : memoryInfo.UllAvailPhys
				};
			}

			private static long GetTotalMemory()
			{
				var memoryInfo = new WindowsMemoryStatus {DwLength = _structLength};
				GlobalMemoryStatusEx(ref memoryInfo);
				return memoryInfo.Equals(default) ? 0 : (memoryInfo.UllTotalPhys);
			}

			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			[SuppressMessage("ReSharper", "InconsistentNaming")]
			[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
#pragma warning disable 649
			private struct WindowsMemoryStatus
			{
				public uint DwLength; // 当前结构体大小
				public readonly uint DwMemoryLoad; // 当前内存使用率
				public readonly long UllTotalPhys; // 总计物理内存大小
				public readonly long UllAvailPhys; // 可用物理内存大小
				public readonly long UllTotalPageFile; // 总计交换文件大小
				public readonly long UllAvailPageFile; // 总计交换文件大小
				public readonly long UllTotalVirtual; // 总计虚拟内存大小
				public readonly long UllAvailVirtual; // 可用虚拟内存大小
				public readonly long UllAvailExtendedVirtual; // 保留 这个值始终为0
			}

			[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			private static extern bool GlobalMemoryStatusEx(ref WindowsMemoryStatus mi);
		}

		internal static class Linux
		{
			/// <summary>
			/// eg. "MemTotal:      12345 kB"
			/// </summary>
			private static readonly Regex _linuxMemoryInfoLineRegex = new(
				@"^(?<name>[^:]+):\s+(?<value>\d+) kB",
				RegexOptions.Multiline | RegexOptions.Compiled
			);

			private static readonly long _totalMemory;

			static Linux()
			{
				_totalMemory = GetTotalMemory(GetMemInfo());
			}

			public static MachineInfo GetMemoryStatus()
			{
				var free = GetFreeMemory(GetMemInfo());
				return new MachineInfo {Memory = _totalMemory, AvailableMemory = free};
			}

			internal static long GetTotalMemory(string output)
			{
				if (string.IsNullOrWhiteSpace(output))
				{
					return 0;
				}

				var dict = GetDict(output);
				var total = dict["MemTotal"];
				return total;
			}

			internal static long GetFreeMemory(string output)
			{
				if (string.IsNullOrWhiteSpace(output))
				{
					return 0;
				}

				var dict = GetDict(output);
				var free = dict["MemAvailable"];
				return free;
			}

			internal static IDictionary<string, long> GetDict(string output)
			{
				if (string.IsNullOrWhiteSpace(output))
				{
					return new Dictionary<string, long>();
				}

				var lines = output;
				// ReSharper disable once RedundantEnumerableCastCall
				var dict = _linuxMemoryInfoLineRegex.Matches(lines).Cast<Match>().ToDictionary(match =>
						match.Groups["name"].Value.TrimStart()
					, match => (long)int.Parse(match.Groups["value"].Value) * 1024);
				return dict;
			}

			private static string GetMemInfo()
			{
				var path = "/proc/meminfo";
				if (!File.Exists(path))
				{
					return default;
				}

				return File.ReadAllText(path);
			}
		}

		public static async Task<double> GetCpuUsageForCurrentProcess()
		{
			var startTime = DateTime.UtcNow;
			var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
			await Task.Delay(500);

			var endTime = DateTime.UtcNow;
			var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
			var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
			var totalMsPassed = (endTime - startTime).TotalMilliseconds;
			var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
			return cpuUsageTotal * 100;
		}

		public static MachineInfo Current
		{
			get
			{
				lock (typeof(MachineInfo))
				{
					try
					{
						if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
						{
							return Windows.GetMemoryStatus();
						}
						else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
						{
							return OSX.GetMemoryStatus();
						}
						else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
						{
							return Linux.GetMemoryStatus();
						}
						else
						{
							throw new NotImplementedException();
						}
					}
					catch
					{
						return default;
					}
				}
			}
		}
	}
}
