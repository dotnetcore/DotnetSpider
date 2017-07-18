#if !NET_CORE
using System.Net;
#endif

namespace DotnetSpider.Core.Infrastructure
{
	public class SystemInfo
	{
		public static readonly string HostName;

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
