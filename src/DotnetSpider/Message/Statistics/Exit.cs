namespace DotnetSpider.Message.Statistics
{
    public class Exit : MessageQueue.Message
    {
        public string SpiderId { get; set; }

        public Exit()
        {
        }

        public Exit(string spiderId)
        {
            SpiderId = spiderId;
        }
    }
}
