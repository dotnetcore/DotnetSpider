namespace DotnetSpider.Message.Statistics
{
    public class Start : MessageQueue.Message
    {
        public string SpiderId { get; set; }
        public string SpiderName { get; set; }

        public Start()
        {
        }

        public Start(string spiderId, string spiderName)
        {
            SpiderId = spiderId;
            SpiderName = spiderName;
        }
    }
}
