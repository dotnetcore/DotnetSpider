using DotnetSpider.Core;

namespace DotnetSpider.Downloader.Entity
{
    /// <summary>
    /// 分配下载器代理的消息
    /// </summary>
    public class AllotDownloaderMessage
    {
        /// <summary>
        /// 爬虫标识
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// 下载器类别
        /// </summary>
        public DownloaderType Type { get; set; }
        
        public Cookie[]  Cookies { get; set; }
        
        public bool UseProxy { get; set; }
        
        public bool UseCookies { get; set; }        
        
        public bool AllowAutoRedirect { get; set; }
        
        public int Timeout { get; set; }
        
        public bool DecodeHtml { get; set; }

        public int DownloaderCount { get; set; }

        public int RetryTimes { get; set; } = 3;
    }
}