using DotnetSpider.Core.Infrastructure;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Proxy
{
	public interface IProxyValidator
	{
		bool IsAvailable(UseSpecifiedUriWebProxy proxy);
	}

	public class DefaultProxyValidator : IProxyValidator
	{
		private static readonly ILogger Logger = DLog.GetLogger();
		private readonly string _targetUrl;

		public DefaultProxyValidator(string targetUrl = "http://www.baidu.com")
		{
			_targetUrl = targetUrl;
			if (string.IsNullOrWhiteSpace(_targetUrl))
			{
				throw new SpiderException($"{nameof(targetUrl)} should not be empty/null.");
			}
			if (!Uri.TryCreate(targetUrl, UriKind.RelativeOrAbsolute, out _))
			{
				throw new SpiderException($"{nameof(targetUrl)}  should be an uri.");
			}
		}

		public bool IsAvailable(UseSpecifiedUriWebProxy proxy)
		{
			var timeout = TimeSpan.FromSeconds(1d);
			var validator = new ProxyValidator(proxy);
			var targetAddress = new Uri(this._targetUrl);
			return validator.Validate(targetAddress, timeout) == HttpStatusCode.OK;
		}

		/// <summary>
		/// 代理验证器
		/// 提供代理的验证
		/// </summary>
		private class ProxyValidator
		{
			/// <summary>
			/// 获取代理
			/// </summary>
			public IWebProxy WebProxy { get; private set; }

			/// <summary>
			/// 代理验证器
			/// </summary>
			/// <param name="proxyHost">代理服务器域名或ip</param>
			/// <param name="proxyPort">代理服务器端口</param>
			/// <exception cref="ArgumentNullException"></exception>
			public ProxyValidator(string proxyHost, int proxyPort)
				: this(new HttpProxy(proxyHost, proxyPort))
			{
			}

			/// <summary>
			/// 代理验证器
			/// </summary>
			/// <param name="webProxy">代理</param>
			/// <exception cref="ArgumentNullException"></exception>
			public ProxyValidator(IWebProxy webProxy)
			{
				this.WebProxy = webProxy ?? throw new ArgumentNullException(nameof(webProxy));
			}

			/// <summary>
			/// 使用http tunnel检测代理状态
			/// </summary>
			/// <param name="targetAddress">目标地址，可以是http或https</param>
			/// <param name="timeout">发送或等待数据的超时时间</param>
			/// <exception cref="ArgumentNullException"></exception>
			/// <returns></returns>
			public HttpStatusCode Validate(Uri targetAddress, TimeSpan? timeout = null)
			{
				if (targetAddress == null)
				{
					throw new ArgumentNullException(nameof(targetAddress));
				}
				return Validate(this.WebProxy, targetAddress, timeout);
			}

			/// <summary>
			/// 转换为字符串
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return this.WebProxy.ToString();
			}

			/// <summary>
			/// 使用http tunnel检测代理状态
			/// </summary>
			/// <param name="webProxy">web代理</param>      
			/// <param name="targetAddress">目标地址，可以是http或https</param>
			/// <param name="timeout">发送或等待数据的超时时间</param>
			/// <exception cref="ArgumentNullException"></exception>    
			/// <returns></returns>
			public static HttpStatusCode Validate(IWebProxy webProxy, Uri targetAddress, TimeSpan? timeout = null)
			{
				if (webProxy == null)
				{
					throw new ArgumentNullException(nameof(webProxy));
				}

				var httpProxy = webProxy as HttpProxy;
				if (httpProxy == null)
				{
					httpProxy = HttpProxy.FromWebProxy(webProxy, targetAddress);
				}

				var remoteEndPoint = new DnsEndPoint(httpProxy.Host, httpProxy.Port, AddressFamily.InterNetwork);
				var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				try
				{
					if (timeout.HasValue == true)
					{
						socket.SendTimeout = (int)timeout.Value.TotalMilliseconds;
						socket.ReceiveTimeout = (int)timeout.Value.TotalMilliseconds;
					}
					socket.Connect(remoteEndPoint);

					var request = httpProxy.ToTunnelRequestString(targetAddress);
					var sendBuffer = Encoding.ASCII.GetBytes(request);
					socket.Send(sendBuffer);

					var recvBuffer = new byte[150];
					var length = socket.Receive(recvBuffer);

					var response = Encoding.ASCII.GetString(recvBuffer, 0, length);
					var statusCode = int.Parse(Regex.Match(response, "(?<=HTTP/1.1 )\\d+", RegexOptions.IgnoreCase).Value);
					return (HttpStatusCode)statusCode;
				}

				catch (Exception)
				{
					return HttpStatusCode.ServiceUnavailable;
				}
				finally
				{
					socket.Dispose();
				}
			}
		}

		/// <summary>
		/// 表示http代理信息
		/// </summary>
		private class HttpProxy : IWebProxy
		{
			/// <summary>
			/// 授权字段
			/// </summary>
			private ICredentials credentials;

			/// <summary>
			/// 获取代理服务器域名或ip
			/// </summary>
			public string Host { get; private set; }

			/// <summary>
			/// 获取代理服务器端口
			/// </summary>
			public int Port { get; private set; }

			/// <summary>
			/// 获取代理服务器账号
			/// </summary>
			public string UserName { get; private set; }

			/// <summary>
			/// 获取代理服务器密码
			/// </summary>
			public string Password { get; private set; }

			/// <summary>
			/// 获取或设置授权信息
			/// </summary>
			ICredentials IWebProxy.Credentials
			{
				get
				{
					return this.credentials;
				}
				set
				{
					this.SetCredentialsByInterface(value);
				}
			}

			/// <summary>
			/// http代理信息
			/// </summary>
			/// <param name="proxyAddress">代理服务器地址</param>
			/// <exception cref="ArgumentNullException"></exception>
			/// <exception cref="ArgumentException"></exception>
			/// <exception cref="ArgumentOutOfRangeException"></exception>
			/// <exception cref="UriFormatException"></exception>
			public HttpProxy(string proxyAddress)
				: this(new Uri(proxyAddress ?? throw new ArgumentNullException(nameof(proxyAddress))))
			{
			}

			/// <summary>
			/// http代理信息
			/// </summary>
			/// <param name="proxyAddress">代理服务器地址</param>
			/// <exception cref="ArgumentNullException"></exception>
			public HttpProxy(Uri proxyAddress)
			{
				if (proxyAddress == null)
				{
					throw new ArgumentNullException(nameof(proxyAddress));
				}
				this.Host = proxyAddress.Host;
				this.Port = proxyAddress.Port;
			}

			/// <summary>
			/// http代理信息
			/// </summary>
			/// <param name="host">代理服务器域名或ip</param>
			/// <param name="port">代理服务器端口</param>
			/// <exception cref="ArgumentNullException"></exception>
			public HttpProxy(string host, int port)
			{
				this.Host = host ?? throw new ArgumentNullException(nameof(host));
				this.Port = port;
			}

			/// <summary>
			/// http代理信息
			/// </summary>
			/// <param name="host">代理服务器域名或ip</param>
			/// <param name="port">代理服务器端口</param>
			/// <param name="userName">代理服务器账号</param>
			/// <param name="password">代理服务器密码</param>
			/// <exception cref="ArgumentNullException"></exception>
			public HttpProxy(string host, int port, string userName, string password)
				: this(host, port)
			{
				this.UserName = userName;
				this.Password = password;

				if (string.IsNullOrEmpty(userName + password) == false)
				{
					this.credentials = new NetworkCredential(userName, password);
				}
			}

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
					var networkCredentialsd = value.GetCredential(null, null);
					userName = networkCredentialsd?.UserName;
					password = networkCredentialsd?.Password;
				}

				this.UserName = userName;
				this.Password = password;
				this.credentials = value;
			}

			/// <summary>
			/// 转换Http Tunnel请求字符串
			/// </summary>      
			/// <param name="targetAddress">目标url地址</param>
			/// <exception cref="ArgumentNullException"></exception>
			/// <returns></returns>
			public string ToTunnelRequestString(Uri targetAddress)
			{
				if (targetAddress == null)
				{
					throw new ArgumentNullException(nameof(targetAddress));
				}

				const string CRLF = "\r\n";
				var builder = new StringBuilder()
					.Append($"CONNECT {targetAddress.Host}:{targetAddress.Port} HTTP/1.1{CRLF}")
					.Append($"Host: {targetAddress.Host}:{targetAddress.Port}{CRLF}")
					.Append($"Accept: */*{CRLF}")
					.Append($"Content-Type: text/html{CRLF}")
					.Append($"Proxy-Connection: Keep-Alive{CRLF}")
					.Append($"Content-length: 0{CRLF}");

				if (this.UserName != null && this.Password != null)
				{
					var bytes = Encoding.ASCII.GetBytes($"{this.UserName}:{this.Password}");
					var base64 = Convert.ToBase64String(bytes);
					builder.AppendLine($"Proxy-Authorization: Basic {base64}{CRLF}");
				}
				return builder.Append(CRLF).ToString();
			}

			/// <summary>
			/// 获取代理服务器地址
			/// </summary>
			/// <param name="destination">目标地址</param>
			/// <returns></returns>
			public Uri GetProxy(Uri destination)
			{
				return new Uri(this.ToString());
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

			/// <summary>
			/// 转换为字符串
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return $"http://{this.Host}:{this.Port}/";
			}

			/// <summary>
			/// 从IWebProxy实例转换获得
			/// </summary>
			/// <param name="webProxy">IWebProxy</param>
			/// <param name="targetAddress">目标url地址</param>
			/// <exception cref="ArgumentNullException"></exception>
			/// <returns></returns>
			public static HttpProxy FromWebProxy(IWebProxy webProxy, Uri targetAddress)
			{
				if (webProxy == null)
				{
					throw new ArgumentNullException(nameof(webProxy));
				}

				if (targetAddress == null)
				{
					throw new ArgumentNullException(nameof(targetAddress));
				}

				var proxyAddress = webProxy.GetProxy(targetAddress);
				var httpProxy = new HttpProxy(proxyAddress);
				httpProxy.SetCredentialsByInterface(webProxy.Credentials);

				return httpProxy;
			}
		}
	}
}