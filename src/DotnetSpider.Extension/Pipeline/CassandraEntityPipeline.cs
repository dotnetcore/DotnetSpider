using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Cassandra;

namespace DotnetSpider.Extension.Pipeline
{
	public class CassandraEntityPipeline : BaseEntityPipeline
	{
		private string _connectString;

		public CassandraEntityPipeline(string connectString)
		{
			_connectString = connectString;
		}

		public override void Process(string entityName, List<JObject> datas)
		{
			var cluster = Cluster.Builder().AddContactPoints("192.168.199.208").Build();
			//Create connections to the nodes using a keyspace
			var session = cluster.Connect("sample_keyspace");
			//Execute a query on a connection synchronously
			var rs = session.Execute("SELECT * FROM sample_table");
			//Iterate through the RowSet
			foreach (var row in rs)
			{
				var value = row.GetValue<int>("sample_int_column");
				//do something with the value
			}
		}
	}
}
