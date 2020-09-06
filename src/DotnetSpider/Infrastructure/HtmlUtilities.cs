using System.Collections.Generic;
using HtmlAgilityPack;

namespace DotnetSpider.Infrastructure
{
    public static class HtmlUtilities
    {
        public static void FixAllRelativeHref(HtmlDocument document, string url)
        {
            var hrefNodes = document.DocumentNode.SelectNodes(".//@href");
            if (hrefNodes != null)
            {
                foreach (var node in hrefNodes)
                {
                    var href = node.Attributes["href"].Value;
                    if (!string.IsNullOrWhiteSpace(href) && !href.Contains("http") && !href.Contains("https"))
                    {
                        node.Attributes["href"].Value = UriUtilities.CanonicalizeUrl(href, url);
                    }
                }
            }

            var srcNodes = document.DocumentNode.SelectNodes(".//@src");
            if (srcNodes != null)
            {
                foreach (var node in srcNodes)
                {
                    var src = node.Attributes["src"].Value;
                    if (!string.IsNullOrWhiteSpace(src) && !src.Contains("http") && !src.Contains("https"))
                    {
                        node.Attributes["src"].Value = UriUtilities.CanonicalizeUrl(src, url);
                    }
                }
            }
        }

        public static void RemoveOutboundLinks(HtmlDocument document, params string[] domains)
        {
            var nodes = document.DocumentNode.SelectNodes(".//a");
            if (nodes != null)
            {
                var deleteNodes = new List<HtmlNode>();
                foreach (var node in nodes)
                {
                    var isMatch = false;
                    foreach (var domain in domains)
                    {
                        var href = node.Attributes["href"]?.Value;
                        if (!string.IsNullOrWhiteSpace(href) &&
                            System.Text.RegularExpressions.Regex.IsMatch(href, domain))
                        {
                            isMatch = true;
                            break;
                        }
                    }

                    if (!isMatch)
                    {
                        deleteNodes.Add(node);
                    }
                }

                foreach (var node in deleteNodes)
                {
                    node.Remove();
                }
            }
        }
    }
}
