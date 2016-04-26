using System.Collections.Generic;

namespace Java2Dotnet.Spider.Extension
{
	public interface ILinkSpiderContext
	{
		SpiderContextBuilder GetBuilder();

		Dictionary<string, SpiderContextBuilder> GetNextSpiders();
	}
}