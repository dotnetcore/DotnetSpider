#if !NETSTANDARD
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Fiddler;

namespace DotnetSpider.Extension.Infrastructure
{
	/// <summary>
	/// Fiddler 下载器
	/// </summary>
	public class FiddlerClient : IDisposable
	{
		private readonly string _partUrl;

		/// <summary>
		/// 取得最近一次符合筛选的请求所使用的Header
		/// </summary>
		public string Headers { get; private set; }

		/// <summary>
		/// 取得最近一次符合筛选的请求所发送的Body
		/// </summary>
		public string RequestBodyString { get; private set; }
	
		/// <summary>
		/// 取得最近一次符合筛选的请求所返回的body
		/// </summary>
		public string ResponseBodyString { get; set; }

		/// <summary>
		/// Fiddler 代理监听的端口
		/// </summary>
		public int Port { get; }

		/// <summary>
		/// Fiddler 代理监听的地址
		/// </summary>
		public string Gateway { get; set; }

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="port">监听端口</param>
		/// <param name="partUrl">包含partUrl的链接为需要监听的请求</param>
		public FiddlerClient(int port, string partUrl)
		{
			Port = port;
			_partUrl = partUrl;
		}

		
		/// <summary>
		/// 开起监听
		/// </summary>
		/// <param name="asSystemProxy">是否设置为系统级代理</param>
		/// <param name="decryptSsl">是否解码SSL</param>
		public void StartCapture(bool asSystemProxy = false, bool decryptSsl = true)
		{
			FiddlerApplication.Shutdown();

			try
			{
				FiddlerApplication.oDefaultClientCertificate = new X509Certificate(Path.Combine(Core.Env.BaseDirectory, "FiddlerRoot.cer"));
				FiddlerApplication.Startup(Port, asSystemProxy, decryptSsl, true);
			}
			catch (Exception)
			{
				FiddlerApplication.Shutdown();
				FiddlerApplication.Startup(Port, asSystemProxy, decryptSsl, true);
			}
			FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
			FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;

			//FiddlerApplication.BeforeResponse += FiddlerApplication_AfterSessionComplete;
		}

		/// <summary>
		/// 清空监听到的请求和返回数据
		/// </summary>
		public void Clear()
		{
			RequestBodyString = null;
			ResponseBodyString = null;
		}

		/// <summary>
		/// 停止代理监听
		/// </summary>
		public void StopCapture()
		{
			FiddlerApplication.oProxy.Detach();
		}

		/// <summary>
		/// 重新开启代理
		/// </summary>
		public void ResumeCapture()
		{
			FiddlerApplication.oProxy.Attach();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			//FiddlerApplication.oProxy.Detach();
			FiddlerApplication.Shutdown();
		}

		private void FiddlerApplication_BeforeRequest(Session oSession)
		{
			if (oSession.url.Contains("localhost") || oSession.url.Contains("127.0.0.1"))
			{
				return;
			}
			oSession["X-OverrideGateway"] = Gateway;
		}

		private string GetMachineIpAddress()
		{
			var ipEntryList = Dns.GetHostEntry(Dns.GetHostName());
			return (from ipAddress in ipEntryList.AddressList where ipAddress.ToString().Contains("192.168") select ipAddress.ToString()).FirstOrDefault();
		}

		private void FiddlerApplication_RequestHeadersAvailable(Session oSession)
		{
			if (!oSession.url.Contains(_partUrl)) return;

			Headers = oSession.oRequest.headers.ToString();
			RequestBodyString = oSession.GetRequestBodyAsString();
		}

		private void FiddlerApplication_AfterSessionComplete(Session oSession)
		{
			if (oSession.url.Contains(_partUrl))//&& string.IsNullOrEmpty(this.Headers) remove for index.baidu.com
			{
				Headers = oSession.oRequest.headers.ToString();
				RequestBodyString = oSession.GetRequestBodyAsString();
				ResponseBodyString = oSession.GetResponseBodyAsString();
			}
			else
			{
				//System.Console.WriteLine(oSession.url);
			}
		}
	}
}
#endif