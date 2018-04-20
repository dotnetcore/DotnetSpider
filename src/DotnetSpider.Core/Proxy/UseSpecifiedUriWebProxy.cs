using System;
using System.Net;

namespace DotnetSpider.Core.Proxy
{
	/// <summary>
	/// HTTP代理的封装
	/// </summary>
	public class UseSpecifiedUriWebProxy : IWebProxy
	{
		/// <summary>
		/// 代理地址
		/// </summary>
		public readonly Uri Uri;

		private readonly bool _bypass;

		/// <summary>
		/// 凭证
		/// </summary>
		public ICredentials Credentials { get; set; }

		/// <summary>
		/// Returns the URI of a proxy.
		/// </summary>
		/// <param name="destination">A System.Uri that specifies the requested Internet resource.</param>
		/// <returns>A System.Uri instance that contains the URI of the proxy used to contact destination.</returns>
		public Uri GetProxy(Uri destination) => Uri;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="uri">代理的链接</param>
		/// <param name="credentials">凭证</param>
		/// <param name="bypass"> Indicates that the proxy should not be used for the specified host.</param>
		public UseSpecifiedUriWebProxy(Uri uri, ICredentials credentials = null, bool bypass = false)
		{
			Uri = uri;
			_bypass = bypass;
			Credentials = credentials;
		}

		/// <summary>
		///  Indicates that the proxy should not be used for the specified host.
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		public bool IsBypassed(Uri host) => _bypass;

		public override string ToString()
		{
			return Uri.ToString();
		}
	}
}
