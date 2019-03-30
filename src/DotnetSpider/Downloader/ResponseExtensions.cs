using System;
using DotnetSpider.Core;
using DotnetSpider.Selector;

namespace DotnetSpider.Downloader
{
    public static class ResponseExtensions
    {
        public static ISelectable ToSelectable(this Response response,
            ContentType type = ContentType.Auto, bool removeOutboundLinks = true)
        {
            switch (type)
            {
                case ContentType.Auto:
                {
                    return IsJson(response.RawText)
                        ? new Selectable(response.RawText)
                        : new Selectable(response.RawText, response.Request.Url, removeOutboundLinks);
                }
                case ContentType.Html:
                {
                    return new Selectable(response.RawText, response.Request.Url, removeOutboundLinks);
                }
                case ContentType.Json:
                {
                    if (IsJson(response.RawText))
                    {
                        return new Selectable(response.RawText);
                    }

                    throw new SpiderException("内容不是合法的 Json");
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        private static bool IsJson(string content)
        {
            return content.StartsWith("[") || content.StartsWith("{");
        }
    }
}