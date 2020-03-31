namespace DotnetSpider.Statistics.Message
{
    public class AgentSuccess : Infrastructure.Message
    {
        public string Id { get; set; }
        public int ElapsedMilliseconds { get; set; }
        
        public AgentSuccess()
        {
        }

        public AgentSuccess(string id, int elapsedMilliseconds)
        {
            Id = id;
            ElapsedMilliseconds = elapsedMilliseconds;
        }
    }
}