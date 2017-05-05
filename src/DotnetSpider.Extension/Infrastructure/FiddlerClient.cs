#if !NET_CORE

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Fiddler;

namespace DotnetSpider.Extension.Infrastructure
{
	public class FiddlerClient : IDisposable
	{
		private readonly string _partUrl;

		public string Headers { get; private set; }

		public string RequestBodyString { get; private set; }

		public string ResponseBodyString { get; set; }

		public int Port { get; }

		public string Gateway { get; set; }

		public FiddlerClient(int port, string partUrl)
		{
			Port = port;
			_partUrl = partUrl;
		}

		public void StartCapture(bool asSystemProxy = false, bool decryptSsl = true)
		{
			FiddlerApplication.Shutdown();

			try
			{
				FiddlerApplication.oDefaultClientCertificate = new X509Certificate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FiddlerRoot.cer"));
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

		void FiddlerApplication_BeforeRequest(Session oSession)
		{
			if (oSession.url.Contains("localhost") || oSession.url.Contains("127.0.0.1"))
			{
				return;
			}
			oSession["X-OverrideGateway"] = Gateway;
		}

		// ReSharper disable once UnusedMember.Local
		private string GetMachineIpAddress()
		{
			var ipEntryList = Dns.GetHostEntry(Dns.GetHostName());
			return (from ipAddress in ipEntryList.AddressList where ipAddress.ToString().Contains("192.168") select ipAddress.ToString()).FirstOrDefault();
		}

		// ReSharper disable once UnusedMember.Local
		void FiddlerApplication_RequestHeadersAvailable(Session oSession)
		{
			if (!oSession.url.Contains(_partUrl)) return;

			Headers = oSession.oRequest.headers.ToString();
			RequestBodyString = oSession.GetRequestBodyAsString();
		}

		void FiddlerApplication_AfterSessionComplete(Session oSession)
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

		public void Clear()
		{
			RequestBodyString = null;
			ResponseBodyString = null;
		}

		public void StopCapture()
		{
			FiddlerApplication.oProxy.Detach();
		}

		public void ResumeCapture()
		{
			FiddlerApplication.oProxy.Attach();
		}

		public void Dispose()
		{
			//FiddlerApplication.oProxy.Detach();
			FiddlerApplication.Shutdown();
		}
	}
}
#endif