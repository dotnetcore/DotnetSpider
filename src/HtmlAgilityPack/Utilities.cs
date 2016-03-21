using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlAgilityPack
{
    internal static class Utilities
    {
        public static TValue GetDictionaryValueOrNull<TKey,TValue>(Dictionary<TKey,TValue> dict, TKey key) where TKey: class
        {
            return dict.ContainsKey(key) ? dict[key] : default(TValue);
        }
    }
}
