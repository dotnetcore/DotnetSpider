using DotnetSpider.Core.Infrastructure;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace DotnetSpider.Core.Proxy
{
	public class ProxyUtil
	{
		public static bool ValidateProxy(string ip, int port)
		{
			bool isReachable = false;

			try
			{
				TcpClient tcp = new TcpClient();
				IPAddress ipAddr = IPAddress.Parse(ip);
				tcp.ReceiveTimeout = 5000;
				tcp.ConnectAsync(ipAddr, port).Wait();
				isReachable = true;
			}
			catch (Exception e)
			{
				LogCenter.Log(null, $"FAILRE - CAN not connect! Proxy: {ip}:{port}.", LogLevel.Error, e);
			}

			return isReachable;
		}
	}
}