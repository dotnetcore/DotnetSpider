using System;
using System.Threading.Tasks;

namespace DotnetSpider.Proxy
{
    public interface IProxyValidator
    {
        Task<bool> IsAvailable(Uri proxy);
    }
}
