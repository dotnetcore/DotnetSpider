namespace DotnetSpider.Statistics.Message
{
    public class Exit : Infrastructure.Message
    {
        public string Id { get; set; }
        
        public Exit()
        {
        }

        public Exit(string id)
        {
            Id = id;
        }
    }
}