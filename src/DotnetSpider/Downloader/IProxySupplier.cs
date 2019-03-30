using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// 代理提供接口
    /// </summary>
    public interface IProxySupplier
    {
        /// <summary>
        /// 取得所有代理
        /// </summary>
        /// <returns>代理</returns>
        Task<Dictionary<string, Proxy>> GetProxies();
    }
}