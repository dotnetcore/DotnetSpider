using System.IO;
using System.Runtime.InteropServices;
using DotnetSpider.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DotnetSpider.Tests
{
	public class SystemInformationTests
	{
		private readonly ITestOutputHelper _testOutput;

		public SystemInformationTests(ITestOutputHelper testOutput)
		{
			_testOutput = testOutput;
		}

		[Fact()]
		public void GetSystemInformation()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				_testOutput.WriteLine(File.ReadAllText("/proc/meminfo"));
			}

			var memoryStatus = MachineInfo.Current;

			_testOutput.WriteLine(
				$"Free: {memoryStatus.AvailableMemory}, Total: {memoryStatus.Memory}");
			Assert.True(memoryStatus.AvailableMemory > 0);
			Assert.True(memoryStatus.Memory > 0);
		}

		[Fact]
		public void LinuxParser()
		{
			var msg = @"MemTotal:        7733016 kB
MemFree:          179152 kB
MemAvailable:    3635216 kB
Buffers:         1141108 kB
Cached:          1728252 kB
SwapCached:            0 kB
Active:          4945628 kB
Inactive:        1112120 kB
Active(anon):    3476156 kB
Inactive(anon):    35872 kB
Active(file):    1469472 kB
Inactive(file):  1076248 kB
Unevictable:          12 kB
Mlocked:              12 kB
SwapTotal:             0 kB
SwapFree:              0 kB
Dirty:               136 kB
Writeback:             0 kB
AnonPages:       3188416 kB
Mapped:           445348 kB
Shmem:            323636 kB
Slab:            1288188 kB
SReclaimable:    1215212 kB
SUnreclaim:        72976 kB
KernelStack:       12720 kB
PageTables:        37560 kB
NFS_Unstable:          0 kB
Bounce:                0 kB
WritebackTmp:          0 kB
CommitLimit:     3866508 kB
Committed_AS:   12078760 kB
VmallocTotal:   34359738367 kB
VmallocUsed:       23504 kB
VmallocChunk:   34359676304 kB
Percpu:             1616 kB
HardwareCorrupted:     0 kB
AnonHugePages:    598016 kB
CmaTotal:              0 kB
CmaFree:               0 kB
HugePages_Total:       0
HugePages_Free:        0
HugePages_Rsvd:        0
HugePages_Surp:        0
Hugepagesize:       2048 kB
DirectMap4k:      123576 kB
DirectMap2M:     3790848 kB
DirectMap1G:     4194304 kB";

			var total = MachineInfo.Linux.GetTotalMemory(msg);
			var free = MachineInfo.Linux.GetFreeMemory(msg);
			Assert.Equal(7551, total);
			Assert.Equal(3550, free);
		}
	}
}
