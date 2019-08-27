using System;
using System.Threading.Tasks;
using DotnetSpider.Common;
using Microsoft.Extensions.Logging;

namespace DotnetSpider.Downloader
{
	/// <summary>
	/// 下载器
	/// </summary>
    public interface IDownloader : IDisposable
    {
	    /// <summary>
	    /// 日志接口
	    /// </summary>
        ILogger Logger { get; set; }

	    /// <summary>
	    /// 下载器代理标识
	    /// </summary>
        string AgentId { get; set; }

	    /// <summary>
	    /// 最后一次使用时间
	    /// </summary>
	    DateTimeOffset LastUsedTime { get; set; }

	    /// <summary>
	    /// 添加 Cookie
	    /// </summary>
	    /// <param name="cookies"></param>
        void AddCookies(params Cookie[] cookies);

	    /// <summary>
	    /// 代理池
	    /// </summary>
        IHttpProxyPool HttpProxyPool { get; set; }

	    /// <summary>
	    /// 下载
	    /// </summary>
	    /// <param name="request">请求</param>
	    /// <returns></returns>
        Task<Response> DownloadAsync(Request request);
    }
}
