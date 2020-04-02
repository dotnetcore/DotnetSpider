namespace DotnetSpider.AgentRegister.Message
{
    public class Register : Infrastructure.Message
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
    }
}