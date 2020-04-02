using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DotnetSpider.Proxy
{
    public interface IProxyPool
    {
        Task<HttpProxy> GetAsync(int seconds);
        Task ReturnAsync(HttpProxy proxy, HttpStatusCode statusCode);
        Task LoadAsync(IEnumerable<HttpProxy> proxies);
    }
}