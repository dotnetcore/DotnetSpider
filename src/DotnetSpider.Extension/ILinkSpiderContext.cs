using System.Collections.Generic;
using DotnetSpider.Extension.Configuration;

namespace DotnetSpider.Extension
{
	public interface ILinkSpiderContext
	{
		SpiderContext GetBuilder();

		Dictionary<string, SpiderContext> GetNextSpiders();
	}
}