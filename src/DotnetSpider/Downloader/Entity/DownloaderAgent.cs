using System;

namespace DotnetSpider.Downloader.Entity
{
	/// <summary>
	/// 下载器代理
	/// </summary>
    public class DownloaderAgent
    {
	    /// <summary>
	    /// 标识
	    /// </summary>
        public string Id { get; set; }

	    /// <summary>
	    /// 名称
	    /// </summary>
        public string Name { get; set; }

	    /// <summary>
	    /// CPU 核心数
	    /// </summary>
        public int ProcessorCount { get; set; }

	    /// <summary>
	    /// 总内存
	    /// </summary>
        public int TotalMemory { get; set; }

	    /// <summary>
	    /// 上一次更新时间
	    /// </summary>
        public DateTime LastModificationTime { get; set; }
    }
}