namespace DotnetSpider.Message.Agent
{
    public class Exit : MessageQueue.Message
    {
        public string AgentId { get; set; }
    }
}
