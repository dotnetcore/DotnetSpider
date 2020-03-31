namespace DotnetSpider.Statistics.Message
{
    public class AgentFailure : Infrastructure.Message
    {
        public string Id { get; set; }
        public int ElapsedMilliseconds { get; set; }

        public AgentFailure()
        {
        }

        public AgentFailure(string id, int elapsedMilliseconds)
        {
            Id = id;
            ElapsedMilliseconds = elapsedMilliseconds;
        }
    }
}