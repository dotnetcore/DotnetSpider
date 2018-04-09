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

        public bool IsAvailable(UseSpecifiedUriWebProxy proxy)
        {
            var userName = default(string);
            var password = default(string);

            if (proxy.Credentials != null)
            {
                var credentials = proxy.Credentials.GetCredential(null, null);
                userName = credentials?.UserName;
                password = credentials?.Password;
            }

            // 目标服务器，IWebProxy无法提供，固定使用baidu
            var targetUri = new Uri("https://www.baidu.com");
            var status = ProxyValidate.Validate(proxy.Uri.Host, proxy.Uri.Port, userName, password, targetUri);
            return status == HttpStatusCode.OK;
        }

        /// <summary>
        /// 提供代理的验证
        /// </summary>
        private static class ProxyValidate
        {
            /// <summary>
            /// 使用http tunnel检测代理状态
            /// </summary>
            /// <param name="proxyHost">代理服务器域名或ip</param>
            /// <param name="proxyPort">代理服务器端口</param>
            /// <param name="targetAddress">目标url地址</param>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            /// <returns></returns>
            public static HttpStatusCode Validate(string proxyHost, int proxyPort, Uri targetAddress)
            {
                return Validate(proxyHost, proxyPort, null, null, targetAddress);
            }

            /// <summary>
            /// 使用http tunnel检测代理状态
            /// </summary>
            /// <param name="proxyHost">代理服务器域名或ip</param>
            /// <param name="proxyPort">代理服务器端口</param>
            /// <param name="userName">代理账号</param>
            /// <param name="password">代理密码</param>
            /// <param name="targetAddress">目标url地址</param>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            /// <returns></returns>
            public static HttpStatusCode Validate(string proxyHost, int proxyPort, string userName, string password, Uri targetAddress)
            {
                var remoteEndPoint = new DnsEndPoint(proxyHost, proxyPort, AddressFamily.InterNetwork);
                var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    socket.Connect(remoteEndPoint);

                    var request = BuildHttpTunnelRequestString(proxyHost, proxyPort, userName, password, targetAddress);
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

            /// <summary>
            /// 生成Http Tunnel请求字符串
            /// </summary>
            /// <param name="proxyHost">代理服务器域名或ip</param>
            /// <param name="proxyPort">代理服务器端口</param>
            /// <param name="userName">代理账号</param>
            /// <param name="password">代理密码</param>
            /// <param name="targetAddress">目标url地址</param>
            /// <exception cref="ArgumentNullException"></exception>
            /// <returns></returns>
            private static string BuildHttpTunnelRequestString(string proxyHost, int proxyPort, string userName, string password, Uri targetAddress)
            {
                if (proxyHost == null)
                {
                    throw new ArgumentNullException(nameof(proxyHost));
                }

                if (targetAddress == null)
                {
                    throw new ArgumentNullException(nameof(targetAddress));
                }

                var builder = new StringBuilder()
                    .AppendLine($"CONNECT {targetAddress.Authority} HTTP/1.1")
                    .AppendLine($"Host: {targetAddress.Authority}")
                    .AppendLine("Accept: */*")
                    .AppendLine("Content-Type: text/html")
                    .AppendLine("Proxy-Connection: Keep-Alive")
                    .AppendLine("Content-length: 0");

                if (userName != null && password != null)
                {
                    var bytes = Encoding.ASCII.GetBytes($"{userName}:{password}");
                    var base64 = Convert.ToBase64String(bytes);
                    builder.AppendLine($"Proxy-Authorization: Basic {base64}");
                }

                return builder.AppendLine().AppendLine().ToString();
            }
        }
    }
}
