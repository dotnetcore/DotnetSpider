namespace DotnetSpider.Statistics.Message
{
    public class Print : Infrastructure.Message
    {
        public string Id { get; set; }

        public Print(string id)
        {
            Id = id;
        }
    }
}