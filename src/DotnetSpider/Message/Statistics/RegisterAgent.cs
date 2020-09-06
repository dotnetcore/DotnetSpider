namespace DotnetSpider.Message.Statistics
{
	public class RegisterAgent
	{
		public string AgentId { get; set; }
		public string AgentName { get; set; }

		public RegisterAgent()
		{
		}

		public RegisterAgent(string id, string name)
		{
			AgentId = id;
			AgentName = name;
		}
	}
}
