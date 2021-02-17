using DotnetSpider.Infrastructure;
using Xunit;

namespace DotnetSpider.Tests
{
	public class SystemInformationTests
	{
		[Fact()]
		public void GetSystemInformation()
		{
			var memoryStatus = SystemInformation.MemoryStatus;

			Assert.True(memoryStatus.FreeMemory > 0);
			Assert.True(memoryStatus.TotalMemory > 0);
			Assert.True(memoryStatus.UsedMemory > 0);
		}
	}
}
