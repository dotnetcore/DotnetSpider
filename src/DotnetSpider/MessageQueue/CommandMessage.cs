namespace DotnetSpider.MessageQueue
{
    /// <summary>
    /// 命令消息
    /// </summary>
    public class CommandMessage
    {
        /// <summary>
        /// 命令
        /// </summary>
        public string Command { get; set; }
        
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}