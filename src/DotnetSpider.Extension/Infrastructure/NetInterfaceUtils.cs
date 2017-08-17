#if !NET_CORE
using System.Management;

namespace DotnetSpider.Extension.Infrastructure
{
	public class NetInterfaceUtils
	{
		public static bool ChangeNetworkConnectionStatus(bool enable, string networkConnectionName)
		{
			uint retRslt = 1;
			using (ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter=TRUE AND NetConnectionID='" + networkConnectionName + "';"))
			{
				foreach (var o in mos.Get())
				{
					var mbo = (ManagementObject)o;
					retRslt = (uint)(enable ? mbo.InvokeMethod("Enable", null) : mbo.InvokeMethod("Disable", null));
					if (retRslt != 0) break;
				}
				if (retRslt == 0)
					return true;
				return false;
			}
		}
	}
}

#endif