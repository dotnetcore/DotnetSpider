using DotnetSpider.Core.Infrastructure;
using StackExchange.Redis;
using System;

namespace DotnetSpider.Extension.Infrastructure
{
	public class RedisConnection
	{
		private static readonly Lazy<RedisConnection> MyInstance = new Lazy<RedisConnection>(() =>
		{
			RedisConnection conn = null;
			var redisConnectString = Config.RedisConnectString;
			if (!string.IsNullOrEmpty(redisConnectString))
			{
				conn = new RedisConnection(redisConnectString);
			}
			return conn;
		});

		public static RedisConnection Default => MyInstance.Value;

		public string ConnectString { get; }
		public IDatabase Database { get; }
		public ISubscriber Subscriber { get; }

		public RedisConnection(string connectString)
		{
			ConnectString = connectString;

			var connection = ConnectionMultiplexer.Connect(connectString);
			Database = connection.GetDatabase(0);
			Subscriber = connection.GetSubscriber();
		}
	}
}