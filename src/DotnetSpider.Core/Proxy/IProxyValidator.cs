using DotnetSpider.Core.Infrastructure;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace DotnetSpider.Core.Proxy
{
	public interface IProxyValidator
	{
		bool IsAvailable(UseSpecifiedUriWebProxy proxy);
	}

	public class DefaultProxyValidator : IProxyValidator
	{
		private static readonly ILogger Logger = DLog.GetLogger();

		public bool IsAvailable(UseSpecifiedUriWebProxy proxy)
		{
			bool isReachable = false;
			var ip = proxy.Uri.Host;
			var port = proxy.Uri.Port;
			try
			{
				TcpClient tcp = new TcpClient();
				IPAddress ipAddr = IPAddress.Parse(ip);
				tcp.ReceiveTimeout = 5000;
				Stopwatch watch = new Stopwatch();
				watch.Start();
				tcp.ConnectAsync(ipAddr, port).Wait();
				watch.Stop();
				Logger.Log($"Detect one avaliable proxy: {ip}:{port}, cost {watch.ElapsedMilliseconds}ms.", Level.Debug);
				isReachable = true;
			}
			catch (Exception e)
			{
				Logger.Log($"Connect test failed for proxy: {ip}:{port}.", Level.Error, e);
			}

			return isReachable;
		}
	}
}
