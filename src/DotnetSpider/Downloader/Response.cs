namespace DotnetSpider.Downloader
{
    public class Response
    {
        /// <summary>
        /// 请求链接
        /// </summary>
        public Request Request { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// 下载内容
        /// </summary>
        public string RawText { get; set; }

        /// <summary>
        /// 下载器代理标识
        /// </summary>
        public string AgentId { get; set; }
        
        /// <summary>
        /// 最终请求的链接, 当发生302跳转时可能与请求的Url不一致
        /// </summary>
        public string TargetUrl { get; set; }        

        /// <summary>
        /// 是否下载成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 下载消耗的时间
        /// </summary>
        public long ElapsedMilliseconds { get; set; }
    }
}