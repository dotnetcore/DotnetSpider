using StackExchange.Redis;
using System;

namespace DotnetSpider.Extension.Infrastructure
{
	public class RedisConnection
	{
		private static readonly Lazy<RedisConnection> MyInstance = new Lazy<RedisConnection>(() =>
		{
			RedisConnection conn = null;
			if (!string.IsNullOrEmpty(Core.Env.RedisConnectString))
			{
				conn = new RedisConnection(Core.Env.RedisConnectString);
			}
			return conn;
		});

		public static RedisConnection Default => MyInstance.Value;

		public IDatabase Database { get; }

		public ISubscriber Subscriber { get; }

		public RedisConnection(string connectString)
		{
			var connection = ConnectionMultiplexer.Connect(connectString);
			Database = connection.GetDatabase();
			Subscriber = connection.GetSubscriber();
		}
	}
}