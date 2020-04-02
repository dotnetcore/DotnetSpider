using System.Threading.Tasks;

namespace DotnetSpider.Proxy
{
    public interface IProxyValidator
    {
        Task<bool> IsAvailable(HttpProxy proxy);
    }
}