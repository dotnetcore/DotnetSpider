using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotnetSpider.DownloadAgentRegisterCenter.Entity
{
	/// <summary>
	/// 下载器代理心跳
	/// </summary>
	[Table("downloader_agent_heartbeat")]
    public class DownloaderAgentHeartbeat
    {
	    [Column("id")]
	    public int Id { get; set; }
	    
	    /// <summary>
	    /// 标识
	    /// </summary>
	    [Column("agent_id")]
	    public string AgentId { get; set; }

	    /// <summary>
	    /// 名称
	    /// </summary>
	    [Column("agent_name")]
	    public string AgentName { get; set; }

	    /// <summary>
	    /// 空闲内存
	    /// </summary>
	    [Column("free_memory")]
        public int FreeMemory { get; set; }

	    /// <summary>
	    /// 已经分配的下载器数量
	    /// </summary>
	    [Column("downloader_count")]
        public int DownloaderCount { get; set; }

	    /// <summary>
	    /// 上报时间
	    /// </summary>
	    [Column("creation_time")]
        public DateTime CreationTime { get; set; }
    }
}