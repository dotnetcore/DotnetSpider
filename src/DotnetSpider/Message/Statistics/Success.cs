namespace DotnetSpider.Message.Statistics
{
    public class Success : MessageQueue.Message
    {
        public string SpiderId { get; set; }

        public Success()
        {
        }

        public Success(string spiderId)
        {
            SpiderId = spiderId;
        }
    }
}
