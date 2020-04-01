namespace DotnetSpider.Statistics.Message
{
	public class RegisterAgent
	{
		public string Id { get; set; }
		public string Name { get; set; }

		public RegisterAgent()
		{
		}

		public RegisterAgent(string id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}
