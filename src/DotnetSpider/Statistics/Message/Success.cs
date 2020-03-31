namespace DotnetSpider.Statistics.Message
{
    public class Success : Infrastructure.Message
    {
        public string Id { get; set; }

        public Success()
        {
        }

        public Success(string id)
        {
            Id = id;
        }
    }
}