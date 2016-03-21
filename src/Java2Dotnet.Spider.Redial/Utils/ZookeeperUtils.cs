#if !NET_CORE

using System;
using System.Configuration;
using System.Threading;
using ZooKeeperNet;

namespace Java2Dotnet.Spider.Redial.Utils
{
	public class ZookeeperUtil
	{
		public static ZooKeeper GetLongSessionZk()
		{
			var zk = new ZooKeeper(ConfigurationManager.AppSettings["zookeeperhost"], TimeSpan.FromDays(23), null);
			Thread.Sleep(500);
			return zk;
		}

		public static ZooKeeper GetShortSessionZk(int seconds = 60)
		{
			var zk = new ZooKeeper(ConfigurationManager.AppSettings["zookeeperhost"], TimeSpan.FromSeconds(seconds), null);
			Thread.Sleep(500);
			return zk;
		}
	}
}
#endif