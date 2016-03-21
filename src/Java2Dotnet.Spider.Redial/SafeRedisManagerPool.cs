//using System;
//using System.Collections.Generic;
//using System.Threading;

//namespace Java2Dotnet.Spider.Redial
//{
//	public class SafeRedisManagerPool : RedisManagerPool
//	{
//		public string Password { get; set; }

//		public SafeRedisManagerPool(string host, string password) : base(host)
//		{
//			Password = password;

//			SetRedisResolver();
//		}

//		public SafeRedisManagerPool(IEnumerable<string> hosts, RedisPoolConfig config,string password) : base(hosts, config)
//		{
//			Password = password;

//			SetRedisResolver();
//		}

//		public IRedisClient GetSafeGetClient()
//		{
//			while (true)
//			{
//				try
//				{
//					return GetClient();
//				}
//				catch (Exception e)
//				{
//					Console.WriteLine("Error: Get redis client failed.");
//					Thread.Sleep(500);
//				}
//			}
//		}

//		private void SetRedisResolver()
//		{
//			RedisResolver.ClientFactory = endpoint =>
//			{
//				endpoint.Password = Password;
//				return new RedisClient(endpoint);
//			};
//		}
//	}
//}
