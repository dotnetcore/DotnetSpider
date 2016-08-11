using System.Collections.Generic;
using DotnetSpider.Extension.Configuration;

namespace DotnetSpider.Extension
{
	public interface ILinkedEntitySpider
	{
		EntitySpider GetSpider();

		Dictionary<string, EntitySpider> GetNextSpider();
	}
}