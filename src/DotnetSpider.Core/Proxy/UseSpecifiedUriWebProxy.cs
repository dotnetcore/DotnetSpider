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
		/// 授权字段
		/// </summary>
		private ICredentials _credentials;

		/// <summary>
		/// 代理地址
		/// </summary>
		public readonly Uri Uri;

		/// <summary>
		/// 获取或设置授权信息
		/// </summary>
		ICredentials IWebProxy.Credentials
		{
			get => _credentials;
			set => SetCredentialsByInterface(value);
		}

		/// <summary>
		/// 获取代理服务器域名或ip
		/// </summary>
		public string Host { get; }

		/// <summary>
		/// 获取代理服务器端口
		/// </summary>
		public int Port { get; }

		/// <summary>
		/// 获取代理服务器账号
		/// </summary>
		public string UserName { get; private set; }

		/// <summary>
		/// 获取代理服务器密码
		/// </summary>
		public string Password { get; private set; }

		public string Hash { get; }

		/// <summary>
		/// Returns the URI of a proxy.
		/// </summary>
		/// <param name="destination">A System.Uri that specifies the requested Internet resource.</param>
		/// <returns>A System.Uri instance that contains the URI of the proxy used to contact destination.</returns>
		public Uri GetProxy(Uri destination) => Uri;

		/// <summary>
		/// http代理信息
		/// </summary>
		/// <param name="proxyAddress">代理服务器地址</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="UriFormatException"></exception>
		public UseSpecifiedUriWebProxy(string proxyAddress)
			: this(new Uri(proxyAddress ?? throw new ArgumentNullException(nameof(proxyAddress))))
		{
		}

		/// <summary>
		/// http代理信息
		/// </summary>
		/// <param name="proxyAddress">代理服务器地址</param>
		/// <exception cref="ArgumentNullException"></exception>
		public UseSpecifiedUriWebProxy(Uri proxyAddress)
		{
			if (proxyAddress == null)
			{
				throw new ArgumentNullException(nameof(proxyAddress));
			}

			Host = proxyAddress.Host;
			Port = proxyAddress.Port;
			Uri = proxyAddress;
			Hash = CalculateHash().ToString();
		}

		/// <summary>
		/// http代理信息
		/// </summary>
		/// <param name="host">代理服务器域名或ip</param>
		/// <param name="port">代理服务器端口</param>
		/// <exception cref="ArgumentNullException"></exception>
		public UseSpecifiedUriWebProxy(string host, int port)
		{
			Host = host ?? throw new ArgumentNullException(nameof(host));
			Port = port;
			Uri = new Uri($"http://{host}:{port}");
			Hash = CalculateHash().ToString();
		}

		/// <summary>
		/// http代理信息
		/// </summary>
		/// <param name="host">代理服务器域名或ip</param>
		/// <param name="port">代理服务器端口</param>
		/// <param name="userName">代理服务器账号</param>
		/// <param name="password">代理服务器密码</param>
		/// <exception cref="ArgumentNullException"></exception>
		public UseSpecifiedUriWebProxy(string host, int port, string userName, string password)
			: this(host, port)
		{
			UserName = userName;
			Password = password;

			if (string.IsNullOrEmpty(userName + password) == false)
			{
				_credentials = new NetworkCredential(userName, password);
			}
		}

		/// <summary>
		/// 是否忽略代理
		/// </summary>
		/// <param name="host">目标地址</param>
		/// <returns></returns>
		public bool IsBypassed(Uri host)
		{
			return false;
		}

		public override string ToString()
		{
			return $"http://{Host}:{Port}/";
		}

		/// <summary>
		/// 获取哈希值
		/// </summary>
		/// <returns></returns>
		private int CalculateHash() => $"{Host}{Port}{UserName}{Password}".GetHashCode();


		/// <summary>
		/// 通过接口设置授权信息
		/// </summary>
		/// <param name="value"></param>
		private void SetCredentialsByInterface(ICredentials value)
		{
			var userName = default(string);
			var password = default(string);
			if (value != null)
			{
				var networkCredentialsd = value.GetCredential(new Uri(Host), string.Empty);
				userName = networkCredentialsd?.UserName;
				password = networkCredentialsd?.Password;
			}

			UserName = userName;
			Password = password;
			_credentials = value;
		}
	}
}