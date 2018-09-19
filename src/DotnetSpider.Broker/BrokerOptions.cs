using System.Collections.Generic;

namespace DotnetSpider.Broker
{
	public class BrokerOptions
	{
		public const string TokenHeader = "Token";
		public bool UseToken { get; set; }
		public HashSet<string> Tokens { get; set; }
		public string ConnectionString { get; set; }
	}
}
