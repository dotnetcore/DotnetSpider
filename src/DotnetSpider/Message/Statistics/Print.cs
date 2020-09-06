namespace DotnetSpider.Message.Statistics
{
    public class Print : MessageQueue.Message
    {
        public string SpiderId { get; set; }

        public Print(string spiderId)
        {
            SpiderId = spiderId;
        }
    }
}
