using NLog;
using System;
using System.Net;
using System.Net.Sockets;

namespace DotnetSpider.Core.Proxy
{
	public class ProxyUtil
	{
		private static IPAddress _localAddr;
		private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

		static ProxyUtil()
		{
			Init();
		}

		private static void Init()
		{
			try
			{
				string hostName = Dns.GetHostName();//本机名   

				IPAddress[] addressList = Dns.GetHostAddressesAsync(hostName).Result;//会返回所有地址，包括IPv4和IPv6   
				foreach (IPAddress ip in addressList)
				{
					if (ip.AddressFamily == AddressFamily.InterNetwork)
					{
						_localAddr = ip;
						break;
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error(e, "Failure when init ProxyUtil");
			}
		}

		public static bool ValidateProxy(HttpHost p)
		{
			if (_localAddr == null)
			{
				Logger.Error("cannot get local ip");
				return false;
			}
			bool isReachable = false;
			Socket socket = null;
			try
			{
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IPv4);

				socket.Bind(new IPEndPoint(_localAddr, 0));
				IPAddress address = IPAddress.Parse(p.Host);
				socket.SendTimeout = 3000;
				socket.ReceiveTimeout = 3000;
				socket.Connect(address, p.Port);

				Logger.Info("SUCCESS - connection established! Local: " + _localAddr + " remote: " + p);
				isReachable = true;
			}
			catch (Exception e)
			{
				Logger.Warn(e, "FAILRE - CAN not connect! Local: " + _localAddr + " remote: " + p);
			}
			finally
			{
				try
				{
#if !NET_CORE
					socket?.Close();
#else
					socket?.Dispose();
#endif
				}
				catch (Exception e)
				{
					Logger.Warn(e, "Error occurred while closing socket of validating proxy");
				}
			}
			return isReachable;
		}
	}
}