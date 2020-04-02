using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DotnetSpider.Infrastructure
{
    public static class SystemInformation
    {
        public static readonly int TotalMemory;

        static SystemInformation()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var mStatus = new MemoryStatus();
                GlobalMemoryStatus(ref mStatus);
                TotalMemory = (int) (Convert.ToInt64(mStatus.DwTotalPhys) / 1024 / 1024);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var lines = File.ReadAllLines("/proc/meminfo");
                var infoDict = lines
                    .Select(line => line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Take(2).ToList())
                    .ToDictionary(items => items[0], items => long.Parse(items[1]));
                TotalMemory = (int) (infoDict["MemTotal:"] / 1024);
            }
        }

        private struct MemoryStatus
        {
            /// <summary>
            /// 总的物理内存大小
            /// </summary>
            public ulong DwTotalPhys { get; set; }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatus(ref MemoryStatus lpBuffer);
    }
}