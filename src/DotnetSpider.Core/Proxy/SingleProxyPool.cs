using System.Net;

namespace DotnetSpider.Core.Proxy
{
	/// <summary>
	/// 单代理模式
	/// </summary>
	public class SingleProxyPool : IHttpProxyPool
	{
		private readonly UseSpecifiedUriWebProxy _proxy;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="url">HTTP代理的链接</param>
		public SingleProxyPool(string url)
		{
			_proxy = new UseSpecifiedUriWebProxy(new System.Uri(url));
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="proxy">HTTP代理</param>
		public SingleProxyPool(UseSpecifiedUriWebProxy proxy)
		{
			_proxy = proxy;
		}

		/// <summary>
		/// 从代理池中取一个代理
		/// </summary>
		/// <returns>代理</returns>
		public UseSpecifiedUriWebProxy GetProxy()
		{
			return _proxy;
		}

		/// <summary>
		/// 单代理模式不需要执行还代理操作
		/// </summary>
		/// <param name="proxy">代理</param>
		/// <param name="statusCode">通过此代理请求数据后的返回状态</param>
		public void ReturnProxy(UseSpecifiedUriWebProxy proxy, HttpStatusCode statusCode)
		{
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
		}
	}
}