namespace DotnetSpider.Core
{
	public static class PageExtensions
	{
		public static bool AddToCycleRetry(this Page page)
		{
			page.Request.CycleTriedTimes++;

			if (page.Request.CycleTriedTimes <= page.Request.Site.CycleRetryTimes)
			{
				page.Request.Priority = 0;
				page.AddTargetRequest(page.Request, false);
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
