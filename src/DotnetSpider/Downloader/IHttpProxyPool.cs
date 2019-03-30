using System;
using System.Net;

namespace DotnetSpider.Downloader
{
    /// <summary>
    /// 代理池
    /// </summary>
    public interface IHttpProxyPool : IDisposable
    {
        /// <summary>
        /// 从代理池中取一个代理
        /// </summary>
        /// <returns>代理</returns>
        WebProxy GetProxy();

        /// <summary>
        /// 把代理返回给代理池
        /// </summary>
        /// <param name="proxy">代理</param>
        /// <param name="statusCode">通过此代理请求数据后的返回状态</param>
        void ReturnProxy(WebProxy proxy, HttpStatusCode statusCode);
    }
}