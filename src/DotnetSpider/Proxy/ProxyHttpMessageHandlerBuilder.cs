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
		private readonly IProxyService _proxyService;

		public ProxyHttpMessageHandlerBuilder(IServiceProvider services,
			IProxyService proxyService)
		{
			services.NotNull(nameof(services));
			proxyService.NotNull(nameof(proxyService));

			Services = services;
			_proxyService = proxyService;
		}

		public override HttpMessageHandler PrimaryHandler { get; set; }

		public override IList<DelegatingHandler> AdditionalHandlers => new List<DelegatingHandler>();

		public override IServiceProvider Services { get; }

		public override HttpMessageHandler Build()
		{
			if (PrimaryHandler != null)
			{
				return CreateHandlerPipeline(PrimaryHandler, AdditionalHandlers);
			}

			if (!Name.StartsWith(Const.ProxyPrefix))
			{
				throw new SpiderException(
					"you are using proxy http client builder, but looks like your http client name is incorrect");
			}

			var uri = Name.Replace(Const.ProxyPrefix, string.Empty);
			var handler = new ProxyHttpClientHandler
			{
				UseCookies = true,
				UseProxy = true,
				ProxyService = _proxyService,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				Proxy = new WebProxy(uri)
			};

			DefaultHttpMessageHandlerBuilder.SetServerCertificateCustomValidationCallback(Services, handler);
			PrimaryHandler = handler;

			var result = CreateHandlerPipeline(PrimaryHandler, AdditionalHandlers);
			return result;
		}

		public override string Name { get; set; }
	}
}
