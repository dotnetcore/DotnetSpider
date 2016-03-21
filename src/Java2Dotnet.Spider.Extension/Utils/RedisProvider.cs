#if NET_CORE
using Java2Dotnet.Spider.Common;
#endif
using Java2Dotnet.Spider.Core;
using StackExchange.Redis;
using System;

namespace Java2Dotnet.Spider.Extension.Utils
{
	public class RedisProvider
	{
		public static ConnectionMultiplexer GetProvider()
		{
			try
			{
#if !NET_CORE
				string host = System.Configuration.ConfigurationManager.AppSettings["redisServer"];
				var password = System.Configuration.ConfigurationManager.AppSettings["redisPassword"];
				int port = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["redisPort"]) ? 6379 : int.Parse(System.Configuration.ConfigurationManager.AppSettings["redisPort"]);

#else
				string host = ConfigurationManager.Get("redisServer");
				var password = ConfigurationManager.Get("redisPassword");
				int port = string.IsNullOrEmpty(ConfigurationManager.Get("redisPort")) ? 6379 : int.Parse(ConfigurationManager.Get("redisPort"));
#endif
				return ConnectionMultiplexer.Connect(new ConfigurationOptions()
				{
					ServiceName = host,
					Password = password,
					ConnectTimeout = 5000,
					KeepAlive = 8,
					EndPoints =
					{
						{ host, port }
					}
				});
			}
			catch (Exception e)
			{
				throw new SpiderExceptoin("Can't init redis provider: " + e);
			}
		}

		public static ConnectionMultiplexer GetProvider(string host, int port, string password)
		{
			try
			{
				return ConnectionMultiplexer.Connect(new ConfigurationOptions()
				{
					ServiceName = host,
					Password = password,
					ConnectTimeout = 5000,
					KeepAlive = 8,
					EndPoints =
					{
						{host, port }
					}
				});
			}
			catch (Exception e)
			{
				throw new SpiderExceptoin("Can't init redis provider: " + e);
			}
		}
	}
}
