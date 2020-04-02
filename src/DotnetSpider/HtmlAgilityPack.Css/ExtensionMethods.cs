using System;
using System.Collections.Generic;

namespace DotnetSpider.HtmlAgilityPack.Css
{
    internal static class ExtensionMethods
    {
        public static IEnumerable<TResult> SelectNonNull<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
        {
            foreach (var item in source)
            {
                var value = selector(item);
                if (value != null) yield return value;
            }
        }
    }
}
