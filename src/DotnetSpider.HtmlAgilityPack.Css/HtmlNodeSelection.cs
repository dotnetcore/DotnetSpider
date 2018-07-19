using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace DotnetSpider.HtmlAgilityPack.Css
{
    /// <summary>
    /// Selector API for <see cref="HtmlNode"/>.
    /// </summary>
    /// <remarks>
    /// For more information, see <a href="http://www.w3.org/TR/selectors-api/">Selectors API</a>.
    /// </remarks>
    public static class HtmlNodeSelection
    {
        private static readonly HtmlNodeOps Ops = new HtmlNodeOps();

        /// <summary>
        /// Similar to <see cref="QuerySelectorAll(HtmlNode,string)" /> 
        /// except it returns only the first element matching the supplied 
        /// selector strings.
        /// </summary>
        public static HtmlNode QuerySelector(this HtmlNode node, string selector)
        {
            return node.QuerySelectorAll(selector).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves all element nodes from descendants of the starting 
        /// element node that match any selector within the supplied 
        /// selector strings. 
        /// </summary>
        public static IEnumerable<HtmlNode> QuerySelectorAll(this HtmlNode node, string selector)
        {
            return QuerySelectorAll(node, selector, null);
        }

        /// <summary>
        /// Retrieves all element nodes from descendants of the starting 
        /// element node that match any selector within the supplied 
        /// selector strings. An additional parameter specifies a 
        /// particular compiler to use for parsing and compiling the 
        /// selector.
        /// </summary>
        /// <remarks>
        /// The <paramref name="compiler"/> can be <c>null</c>, in which
        /// case a default compiler is used. If the selector is to be used
        /// often, it is recommended to use a caching compiler such as the
        /// one supplied by <see cref="CreateCachingCompiler()"/>.
        /// </remarks>
        public static IEnumerable<HtmlNode> QuerySelectorAll(this HtmlNode node, string selector, Func<string, Func<HtmlNode, IEnumerable<HtmlNode>>> compiler)
        {
            return (compiler ?? CachableCompile)(selector)(node);
        }

        /// <summary>
        /// Gets or sets the maximum number of compiled selectors that will be kept in cache.
        /// </summary>
        public static int CacheSize
        {
            get => _compilerCache.Capacity;
            set => _compilerCache.Capacity = value;
        }

        /// <summary>
        /// Parses and compiles CSS selector text into run-time function.
        /// </summary>
        /// <remarks>
        /// Use this method to compile and reuse frequently used CSS selectors
        /// without parsing them each time.
        /// </remarks>
        public static Func<HtmlNode, IEnumerable<HtmlNode>> Compile(string selector)
        {
            var compiled = Parser.Parse(selector, new SelectorGenerator<HtmlNode>(Ops)).Selector;
            return node => compiled(Enumerable.Repeat(node, 1));
        }

        //
        // Caching
        //

        private const int DefaultCacheSize = 60;

        private static LruCache<string, Func<HtmlNode, IEnumerable<HtmlNode>>> _compilerCache = new LruCache<string, Func<HtmlNode, IEnumerable<HtmlNode>>>(Compile, DefaultCacheSize);
        private static Func<string, Func<HtmlNode, IEnumerable<HtmlNode>>> _defaultCachingCompiler = _compilerCache.GetValue;
            
        /// <summary>
        /// Compiles a selector. If the selector has been previously 
        /// compiled then this method returns it rather than parsing
        /// and compiling the selector on each invocation.
        /// </summary>
        public static Func<HtmlNode, IEnumerable<HtmlNode>> CachableCompile(string selector)
        {
            return _defaultCachingCompiler(selector);
        }

    }
}
