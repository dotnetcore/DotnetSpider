namespace DotnetSpider.Statistics.Message
{
    public class Failure : Infrastructure.Message
    {
        public string Id { get; set; }

        public Failure()
        {
        }

        public Failure(string id)
        {
            Id = id;
        }
    }
}