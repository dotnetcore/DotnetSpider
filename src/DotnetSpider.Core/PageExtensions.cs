namespace DotnetSpider.Core
{
	public static class PageExtensions
	{
		public static bool AddToCycleRetry(this Page page, int cycleRetryTimes)
		{
			var cycleTriedTimes = page.Request.GetProperty(Page.CycleTriedTimes);
			if (cycleTriedTimes == null)
			{
				cycleTriedTimes = 1;
			}
			else
			{
				cycleTriedTimes += 1;
			}
			page.Request.AddProperty(Page.CycleTriedTimes, cycleTriedTimes);
			if (cycleTriedTimes <= cycleRetryTimes)
			{
				// page.Request.AddProperty(Page.Priority, 0);
				page.Retry = true;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
