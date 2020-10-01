using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;

namespace DotnetSpider.Infrastructure
{
	public class DefaultHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
	{
		public DefaultHttpMessageHandlerBuilder(IServiceProvider services)
		{
			services.NotNull(nameof(services));


			Services = services;
		}

		public override HttpMessageHandler PrimaryHandler { get; set; }

		public override IList<DelegatingHandler> AdditionalHandlers => new List<DelegatingHandler>();

		public override IServiceProvider Services { get; }

		public override HttpMessageHandler Build()
		{
			if (PrimaryHandler == null)
			{
				var handler = new HttpClientHandler
				{
					UseCookies = true,
					AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				};

				SetServerCertificateCustomValidationCallback(Services, handler);
				PrimaryHandler = handler;
			}

			return CreateHandlerPipeline(PrimaryHandler, AdditionalHandlers);
		}

		public override string Name { get; set; }

		public static void SetServerCertificateCustomValidationCallback(IServiceProvider services,
			HttpClientHandler handler)
		{
			var hostBuilderContext = services.GetService<HostBuilderContext>();
			var ignoreSslError = (hostBuilderContext.Properties.ContainsKey(Consts.IgnoreSslError) &&
			                      hostBuilderContext.Properties[Consts.IgnoreSslError]?.ToString().ToLower() == "true")
			                     || Environment.GetEnvironmentVariable(Consts.IgnoreSslError)?.ToLower() == "true";
			if (ignoreSslError)
			{
				handler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;
			}
		}
	}
}
