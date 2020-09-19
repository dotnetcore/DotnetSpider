using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DotnetSpider.Proxy
{
	public interface IProxyService
	{
		/// <summary>
		///
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>
		Task<ProxyEntry> GetAsync(int seconds = 60);
		Task ReturnAsync(ProxyEntry proxy, HttpStatusCode statusCode);
		Task AddAsync(IEnumerable<ProxyEntry> proxies);
	}
}
