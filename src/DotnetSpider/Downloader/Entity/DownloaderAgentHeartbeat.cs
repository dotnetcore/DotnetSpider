using System;

namespace DotnetSpider.Downloader.Entity
{
	/// <summary>
	/// 下载器代理心跳
	/// </summary>
    public class DownloaderAgentHeartbeat
    {
	    public int Id { get; set; }
	    
	    /// <summary>
	    /// 标识
	    /// </summary>
	    public string AgentId { get; set; }

	    /// <summary>
	    /// 名称
	    /// </summary>
	    public string AgentName { get; set; }

	    /// <summary>
	    /// 空闲内存
	    /// </summary>
        public int FreeMemory { get; set; }

	    /// <summary>
	    /// 已经分配的下载器数量
	    /// </summary>
        public int DownloaderCount { get; set; }

	    /// <summary>
	    /// 上报时间
	    /// </summary>
        public DateTime CreationTime { get; set; }
    }
}