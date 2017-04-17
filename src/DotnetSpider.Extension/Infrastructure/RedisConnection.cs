using DotnetSpider.Core;
using DotnetSpider.Core.Infrastructure;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DotnetSpider.Extension.Infrastructure
{
	public class RedisConnection
	{
		public string ConnectString { get; private set; }
		public IDatabase Database { get; private set; }
		public ISubscriber Subscriber { get; private set; }

		public bool IsEnable { get; private set; }

		public RedisConnection(string connectString)
		{
			ConnectString = connectString;

			var _connection = ConnectionMultiplexer.Connect(connectString);
			Database = _connection.GetDatabase(0);
			Subscriber = _connection.GetSubscriber();
		}
	}
}
