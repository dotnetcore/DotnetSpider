using System.Collections.Generic;

namespace DotnetSpider.Extension
{
	public interface ILinkSpiderContext
	{
		SpiderContext GetBuilder();

		Dictionary<string, SpiderContext> GetNextSpiders();
	}
}