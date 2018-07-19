using System.Collections.Generic;

namespace DotnetSpider.Proxy
{
	/// <summary>
	/// 代理提供接口
	/// </summary>
	public interface IProxySupplier
	{
		/// <summary>
		/// 取得所有代理
		/// </summary>
		/// <returns>代理</returns>
		Dictionary<string, ProxyInfo> GetProxies();
	}
}
