namespace DotnetSpider.Message.Statistics
{
	public class AgentSuccess : MessageQueue.Message
	{
		public string AgentId { get; set; }
		public int ElapsedMilliseconds { get; set; }

		public AgentSuccess()
		{
		}

		public AgentSuccess(string agentId, int elapsedMilliseconds)
		{
			AgentId = agentId;
			ElapsedMilliseconds = elapsedMilliseconds;
		}
	}
}
