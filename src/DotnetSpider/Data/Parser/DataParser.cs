using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotnetSpider.Downloader;

namespace DotnetSpider.Data.Parser
{
    /// <summary>
    /// 默认数据解析器
    /// </summary>
    public class DataParser : DataParserBase
    {
        protected override Task<DataFlowResult> Parse(DataFlowContext context)
        {
            if (context.Response != null)
            {
                context.AddItem("URL", context.Response.Request.Url);
                context.AddItem("Content", context.Response.RawText);
                context.AddItem("TargetUrl", context.Response.TargetUrl);
                context.AddItem("Success", context.Response.Success);
                context.AddItem("ElapsedMilliseconds", context.Response.ElapsedMilliseconds);
            }
            return Task.FromResult(DataFlowResult.Success);
        }
        
        public static Func<DataFlowContext, string[]> XPathFollow(params string[] xPaths)
        {
            return context =>
            {
                var urls = new List<string>();
                foreach (var xpath in xPaths)
                {
                    var links = context.GetSelectable().XPath(xpath).Links().GetValues();
                    foreach (var link in links)
                    {
#if !NETSTANDARD
                        urls.Add(System.Web.HttpUtility.HtmlDecode(System.Web.HttpUtility.UrlDecode(link)));
#else
                        urls.Add(WebUtility.HtmlDecode(WebUtility.UrlDecode(link)));
#endif
                    }
                }

                return urls.ToArray();
            };
        }

        public static Func<Request, bool> RegexCanParse(params string[] patterns)
        {
            return request =>
            {
                foreach (var pattern in patterns)
                {
                    if (Regex.IsMatch(request.Url, pattern))
                    {
                        return true;
                    }
                }

                return false;
            };
        }
    }
}