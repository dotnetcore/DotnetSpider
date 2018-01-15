using StackExchange.Redis;
using System;

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// Redis连接信息
	/// </summary>
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

		/// <summary>
		/// Redis连接信息对象
		/// </summary>
		public static RedisConnection Default => MyInstance.Value;

		/// <summary>
		/// Describes functionality that is common to both standalone redis servers and redis clusters
		/// </summary>
		public IDatabase Database { get; }

		/// <summary>
		/// A redis connection used as the subscriber in a pub/sub scenario
		/// </summary>
		public ISubscriber Subscriber { get; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="connectString">连接字符串</param>
		public RedisConnection(string connectString)
		{
			var connection = ConnectionMultiplexer.Connect(connectString);
			Database = connection.GetDatabase();
			Subscriber = connection.GetSubscriber();
		}
	}
}