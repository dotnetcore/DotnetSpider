using System;

namespace DotnetSpider.Infrastructure
{
    public static class UriUtils
    {
        /// <summary>
        /// 计算最终的URL
        /// </summary>
        /// <param name="uri">Base uri</param>
        /// <param name="relativeUri">Relative uri</param>
        /// <returns>最终的URL</returns>
        public static string CanonicalizeUrl(string uri, string relativeUri)
        {
            try
            {
                var baseUri = new Uri(relativeUri);
                if (uri.StartsWith("//"))
                {
                    return $"{baseUri.Scheme}:{uri}";
                }
                else
                {
                    var abs = new Uri(baseUri, uri);
                    return abs.AbsoluteUri;
                }
            }
            catch (Exception)
            {
                return uri;
            }
        }
    }
}