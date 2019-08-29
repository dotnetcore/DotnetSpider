using System.Threading.Tasks;
using Xunit;

namespace DotnetSpider.Tests.Statistics
{
	public class StatisticsTests : TestBase
	{
		/// <summary>
		/// 1. 重试会
		/// </summary>
		[Fact(DisplayName = "SpiderStatistics")]
		public async Task SpiderStatistics()
		{
			var s = new SpiderTests();
			await s.RetryDownloadTimes();
			s.RetryWhenResultIsEmpty();
		}
	}
}