namespace DotnetSpider.Message.Statistics
{
    public class Failure : MessageQueue.Message
    {
        public string SpiderId { get; set; }

        public Failure()
        {
        }

        public Failure(string spiderId)
        {
            SpiderId = spiderId;
        }
    }
}
