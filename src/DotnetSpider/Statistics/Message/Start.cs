namespace DotnetSpider.Statistics.Message
{
    public class Start : Infrastructure.Message
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Start()
        {
        }

        public Start(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}