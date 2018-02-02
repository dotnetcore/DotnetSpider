#if NET45
using System.Management;

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// 网络接口的帮助类
	/// </summary>
	public static class NetInterfaceUtil
	{
		/// <summary>
		/// 更新网络接口的连接状态
		/// </summary>
		/// <param name="enable">启用或停用</param>
		/// <param name="networkConnectionName">网络接口名称</param>
		/// <returns>是否更新成功</returns>
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