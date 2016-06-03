using System.Collections.Generic;

namespace Java2Dotnet.Spider.Extension
{
	public interface ILinkSpiderContext
	{
		SpiderContext GetBuilder();

		Dictionary<string, SpiderContext> GetNextSpiders();
	}
}