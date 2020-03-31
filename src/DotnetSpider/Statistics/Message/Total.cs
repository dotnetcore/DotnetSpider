namespace DotnetSpider.Statistics.Message
{
    public class Total : Infrastructure.Message
    {
        public string Id { get; set; }
        public long Count { get; set; }

        public Total()
        {
        }

        public Total(string id, long count)
        {
            Id = id;
            Count = count;
        }
    }
}