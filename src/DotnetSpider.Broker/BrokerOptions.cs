using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetSpider.Broker
{
	public enum StorageType
	{
		SqlServer,
		MySql
	}

	public class BrokerOptions
	{
		public const string TokenHeader = "Token";
		public bool UseToken { get; set; }
		public HashSet<string> Tokens { get; set; }
		public StorageType StorageType { get; set; }
		public string ConnectionString { get; set; }
	}
}
