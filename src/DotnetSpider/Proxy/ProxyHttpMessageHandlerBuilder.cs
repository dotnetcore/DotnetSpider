using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using DotnetSpider.Infrastructure;
using Microsoft.Extensions.Http;

namespace DotnetSpider.Proxy
{
	public class ProxyHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
	{
		private string _name;

		public ProxyHttpMessageHandlerBuilder(IServiceProvider services)
		{
			Services = services;
		}

		public override string Name
		{
			get => _name;
			set => _name = value ?? throw new ArgumentNullException(nameof(value));
		}

		public override HttpMessageHandler PrimaryHandler { get; set; }

		public override IList<DelegatingHandler> AdditionalHandlers { get; } =
			(IList<DelegatingHandler>)new List<DelegatingHandler>();

		public override IServiceProvider Services { get; }

		public override HttpMessageHandler Build()
		{
			if (PrimaryHandler == null)
			{
				var handler = new HttpClientHandler
				{
					AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
					ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
				};

				if (Name.StartsWith(Consts.ProxyPrefix))
				{
					var uri = new Uri(Name.Replace(Consts.ProxyPrefix, ""));
					var proxy = new WebProxy(uri) {Credentials = new NetworkCredential()};
					handler = new HttpClientHandler {Proxy = proxy};
				}

				SetServerCertificateCustomValidationCallback(handler);
				PrimaryHandler = handler;
			}

			return CreateHandlerPipeline(PrimaryHandler, AdditionalHandlers);
		}

		private void SetServerCertificateCustomValidationCallback(HttpClientHandler handler)
		{
			if (Environment.GetEnvironmentVariable("IGNORE_SSL_ERROR")?.ToLower() == "true")
			{
				handler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;
			}
		}
	}
}
