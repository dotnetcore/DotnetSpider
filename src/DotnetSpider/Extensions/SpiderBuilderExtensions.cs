using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Extensions
{
    public static class SpiderBuilderExtensions
    {
        public static Builder ConfigureServices(
            this Builder builder,
            Action<IServiceCollection> configureDelegate)
        {
            builder.ConfigureServices((context, collection) => configureDelegate(collection));
            return builder;
        }
    }
}