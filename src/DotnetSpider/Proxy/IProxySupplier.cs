using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSpider.Proxy
{
    public interface IProxySupplier
    {
        Task<IEnumerable<HttpProxy>> GetProxiesAsync();
    }
}