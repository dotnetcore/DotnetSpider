using Xunit;

namespace DotnetSpider.Tests.Statistics
{
    public class StatisticsTests : TestBase
    {
        /// <summary>
        /// 1. 重试会
        /// </summary>
        [Fact(DisplayName = "SpiderStatistics")]
        public void SpiderStatistics()
        {
            SpiderTests s = new SpiderTests();
            s.RetryDownloadTimes();
            s.RetryWhenResultIsEmpty();
        }
    }
}