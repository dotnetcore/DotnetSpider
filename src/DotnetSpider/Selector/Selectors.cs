using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace DotnetSpider.Selector
{
    /// <summary>
    /// 查询器的构建帮助类, 相同的查询器会缓存起来.
    /// </summary>
    public class Selectors
    {
        private static readonly ConcurrentDictionary<string, ISelector> Cache =
            new();

        /// <summary>
        /// 创建正则查询器
        /// </summary>
        /// <param name="expr">正则表达式</param>
        /// <param name="options"></param>
        /// <param name="group"></param>
        /// <returns>查询器</returns>
        public static ISelector Regex(string expr,
            RegexOptions options = RegexOptions.None, string replacement = "$0")
        {
            var key = $"r_{expr}_{replacement}";
            if (!Cache.ContainsKey(key))
            {
                Cache.TryAdd(key, new RegexSelector(expr, options, replacement));
            }

            return Cache[key];
        }

        /// <summary>
        /// 创建Css查询器
        /// </summary>
        /// <param name="expr">Css表达式</param>
        /// <param name="attr">属性名称</param>
        /// <returns>查询器</returns>
        public static ISelector Css(string expr, string attr = null)
        {
            var key = $"c_{expr}_{attr}";
            if (!Cache.ContainsKey(key))
            {
                Cache.TryAdd(key, new CssSelector(expr, attr));
            }

            return Cache[key];
        }

        /// <summary>
        /// 创建XPath查询器
        /// </summary>
        /// <param name="expr">Xpath表达式</param>
        /// <returns>查询器</returns>
        public static ISelector XPath(string expr)
        {
            var key = $"x_{expr}";
            if (!Cache.ContainsKey(key))
            {
                Cache.TryAdd(key, new XPathSelector(expr));
            }

            return Cache[key];
        }

        /// <summary>
        /// 创建JsonPath查询器
        /// </summary>
        /// <param name="expr">JsonPath表达式</param>
        /// <returns>查询器</returns>
        public static ISelector JsonPath(string expr)
        {
            var key = $"j_{expr}";
            if (!Cache.ContainsKey(key))
            {
                Cache.TryAdd(key, new JsonPathSelector(expr));
            }

            return Cache[key];
        }
    }
}
