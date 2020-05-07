using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
            (IList<DelegatingHandler>) new List<DelegatingHandler>();

        public override IServiceProvider Services { get; }

        public override HttpMessageHandler Build()
        {
            if (PrimaryHandler == null)
            {
                if (Name.StartsWith(Consts.ProxyPrefix))
                {
                    var uri = Name.Replace(Consts.ProxyPrefix, "");
                    var proxy = new WebProxy(new Uri(uri));
                    PrimaryHandler = new HttpClientHandler
                    {
                        Proxy = proxy,
                    };
                }
                else
                {
                    PrimaryHandler = new HttpClientHandler();
                }
            }

            return CreateHandlerPipeline(PrimaryHandler, AdditionalHandlers);
        }
    }
}
