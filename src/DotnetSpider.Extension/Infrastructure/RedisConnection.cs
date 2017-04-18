using StackExchange.Redis;

namespace DotnetSpider.Extension.Infrastructure
{
	public class RedisConnection
	{
		public string ConnectString { get; private set; }
		public IDatabase Database { get; private set; }
		public ISubscriber Subscriber { get; private set; }

		public RedisConnection(string connectString)
		{
			ConnectString = connectString;

			var _connection = ConnectionMultiplexer.Connect(connectString);
			Database = _connection.GetDatabase(0);
			Subscriber = _connection.GetSubscriber();
		}
	}
}
