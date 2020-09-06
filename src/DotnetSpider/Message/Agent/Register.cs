namespace DotnetSpider.Message.Agent
{
    public class Register : MessageQueue.Message
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
        /// CPU 核心数
        /// </summary>
        public int ProcessorCount { get; set; }

        /// <summary>
        /// 总内存
        /// </summary>
        public int TotalMemory { get; set; }
    }
}
