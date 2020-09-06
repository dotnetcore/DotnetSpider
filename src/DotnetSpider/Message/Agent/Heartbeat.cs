namespace DotnetSpider.Message.Agent
{
    public class Heartbeat : MessageQueue.Message
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
        public int FreeMemory { get; set; }

        /// <summary>
        /// CPU 负载
        /// </summary>
        public int CpuLoad { get; set; }
    }
}
