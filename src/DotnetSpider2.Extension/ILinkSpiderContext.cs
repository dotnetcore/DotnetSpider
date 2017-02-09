using System.Collections.Generic;

namespace DotnetSpider.Extension
{
	public interface ILinkedEntitySpider
	{
		EntitySpider GetSpider();

		Dictionary<string, EntitySpider> GetNextSpider();
	}
}