namespace DotnetSpider.Message.Statistics
{
	public class AgentFailure : MessageQueue.Message
	{
		public string AgentId { get; set; }
		public int ElapsedMilliseconds { get; set; }

		public AgentFailure()
		{
		}

		public AgentFailure(string agentId, int elapsedMilliseconds)
		{
			AgentId = agentId;
			ElapsedMilliseconds = elapsedMilliseconds;
		}
	}
}
