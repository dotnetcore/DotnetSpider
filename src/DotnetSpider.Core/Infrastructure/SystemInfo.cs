using System;
using System.Diagnostics;
using System.Net;
#if NET_CORE
using System.Runtime.InteropServices;
#else
using System.Management;
#endif

namespace DotnetSpider.Core.Infrastructure
{
	public class SystemInfo
	{
		public static readonly string HostName;

#if !NET_CORE
		private static readonly PerformanceCounter PcCpuLoad; //CPU计数器
#endif

		static SystemInfo()
		{
#if !NET_CORE
			HostName = Dns.GetHostName();

			// docker 化后只会有一个IP
			//Ip4Address = Dns.GetHostAddressesAsync(HostName).Result[0].ToString();
#else
			HostName = "Unsport";
#endif
		}
	}
}
