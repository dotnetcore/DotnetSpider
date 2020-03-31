namespace DotnetSpider.AgentRegister.Message
{
    public class Heartbeat : Infrastructure.Message
    {
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
        public uint FreeMemory { get; set; }

        /// <summary>
        /// 已经分配的下载器数量
        /// </summary>
        public uint DownloaderCount { get; set; }
    }
}