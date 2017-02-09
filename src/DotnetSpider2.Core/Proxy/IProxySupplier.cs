using System.Collections.Generic;

namespace DotnetSpider.Core.Proxy
{
	public interface IProxySupplier
	{
		Dictionary<string, Proxy> GetProxies();
	}
}
