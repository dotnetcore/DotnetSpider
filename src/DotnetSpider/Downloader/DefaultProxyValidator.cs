using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using DotnetSpider.Core;

namespace DotnetSpider.Downloader
{
    public class DefaultProxyValidator : IProxyValidator
    {
        private readonly string _targetUrl;

        public DefaultProxyValidator(string targetUrl = "http://www.baidu.com")
        {
            _targetUrl = targetUrl;
            if (string.IsNullOrEmpty(_targetUrl))
            {
                throw new SpiderException($"{nameof(targetUrl)} is empty/null.");
            }

            if (!Uri.TryCreate(targetUrl, UriKind.RelativeOrAbsolute, out _))
            {
                throw new SpiderException($"{nameof(targetUrl)} is not an uri.");
            }
        }

        public bool IsAvailable(WebProxy proxy)
        {
            var timeout = TimeSpan.FromSeconds(1d);
            var validator = new ProxyValidator(proxy);
            var targetAddress = new Uri(_targetUrl);
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
            public IWebProxy WebProxy { get; }

            /// <summary>
            /// 代理验证器
            /// </summary>
            /// <param name="proxyHost">代理服务器域名或ip</param>
            /// <param name="proxyPort">代理服务器端口</param>
            // ReSharper disable once UnusedMember.Local
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
                WebProxy = webProxy ?? throw new ArgumentNullException(nameof(webProxy));
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

                return Validate(WebProxy, targetAddress, timeout);
            }

            /// <summary>
            /// 转换为字符串
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return WebProxy.ToString();
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

                Socket socket = null;
                try
                {
                    var ascii = Encoding.GetEncoding("ASCII");
                    var host = Dns.GetHostEntry(httpProxy.Host);
                    socket = new Socket(host.AddressList[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    if (timeout.HasValue)
                    {
                        socket.SendTimeout = (int) timeout.Value.TotalMilliseconds;
                        socket.ReceiveTimeout = (int) timeout.Value.TotalMilliseconds;
                    }

                    socket.Connect(new IPEndPoint(host.AddressList[0], httpProxy.Port));

                    var request = httpProxy.ToTunnelRequestString(targetAddress);
                    var sendBuffer = ascii.GetBytes(request);
                    socket.Send(sendBuffer);

                    var recvBuffer = new byte[150];
                    var length = socket.Receive(recvBuffer);

                    var response = ascii.GetString(recvBuffer, 0, length);
                    var statusCode =
                        int.Parse(Regex.Match(response, "(?<=HTTP/1.1 )\\d+", RegexOptions.IgnoreCase).Value);
                    return (HttpStatusCode) statusCode;
                }
                catch (Exception)
                {
                    return HttpStatusCode.ServiceUnavailable;
                }
                finally
                {
                    socket?.Dispose();
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
            private ICredentials _credentials;

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

            /// <summary>
            /// 获取或设置授权信息
            /// </summary>
            ICredentials IWebProxy.Credentials
            {
                get => _credentials;
                set => SetCredentialsByInterface(value);
            }

            /// <summary>
            /// http代理信息
            /// </summary>
            /// <param name="proxyAddress">代理服务器地址</param>
            // ReSharper disable once UnusedMember.Local
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

                Host = proxyAddress.Host;
                Port = proxyAddress.Port;
            }

            /// <summary>
            /// http代理信息
            /// </summary>
            /// <param name="host">代理服务器域名或ip</param>
            /// <param name="port">代理服务器端口</param>
            /// <exception cref="ArgumentNullException"></exception>
            public HttpProxy(string host, int port)
            {
                Host = host ?? throw new ArgumentNullException(nameof(host));
                Port = port;
            }

            /// <summary>
            /// http代理信息
            /// </summary>
            /// <param name="host">代理服务器域名或ip</param>
            /// <param name="port">代理服务器端口</param>
            /// <param name="userName">代理服务器账号</param>
            /// <param name="password">代理服务器密码</param>
            // ReSharper disable once UnusedMember.Local
            public HttpProxy(string host, int port, string userName, string password)
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

            /// <summary>
            /// 转换Http Tunnel请求字符串
            /// </summary>      
            /// <param name="targetAddress">目标url地址</param>
            public string ToTunnelRequestString(Uri targetAddress)
            {
                if (targetAddress == null)
                {
                    throw new ArgumentNullException(nameof(targetAddress));
                }

                const string crlf = "\r\n";
                var builder = new StringBuilder()
                    .Append($"CONNECT {targetAddress.Host}:{targetAddress.Port} HTTP/1.1{crlf}")
                    .Append($"Host: {targetAddress.Host}:{targetAddress.Port}{crlf}")
                    .Append($"Accept: */*{crlf}")
                    .Append($"Content-Type: text/html{crlf}")
                    .Append($"Proxy-Connection: Keep-Alive{crlf}")
                    .Append($"Content-length: 0{crlf}");

                if (UserName != null && Password != null)
                {
                    var bytes = Encoding.ASCII.GetBytes($"{UserName}:{Password}");
                    var base64 = Convert.ToBase64String(bytes);
                    builder.AppendLine($"Proxy-Authorization: Basic {base64}{crlf}");
                }

                return builder.Append(crlf).ToString();
            }

            /// <summary>
            /// 获取代理服务器地址
            /// </summary>
            /// <param name="destination">目标地址</param>
            /// <returns></returns>
            public Uri GetProxy(Uri destination)
            {
                return new Uri(ToString());
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
                return $"http://{Host}:{Port}/";
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