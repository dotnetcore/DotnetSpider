using System.Collections.Generic;

namespace Java2Dotnet.Spider.Core.Selector
{
	/// <summary>
	/// Convenient methods for selectors.
	/// </summary>
	public class Selectors
	{
		private static Dictionary<string, ISelector> _cache = new Dictionary<string, ISelector>();

		static Selectors()
		{
			_cache.Add("SmartContentSelector", new SmartContentSelector());
		}

		public static ISelector Regex(string expr)
		{
			if (!_cache.ContainsKey(expr))
			{
				_cache.Add(expr, new RegexSelector(expr));
			}
			return _cache[expr];
		}

		public static ISelector Css(string expr)
		{
			if (!_cache.ContainsKey(expr))
			{
				_cache.Add(expr, new CssHtmlSelector(expr));
			}
			return _cache[expr];
		}

		public static ISelector Css(string expr, string attrName)
		{
			if (!_cache.ContainsKey(expr + attrName))
			{
				_cache.Add(expr + attrName, new CssHtmlSelector(expr, attrName));
			}
			return _cache[expr + attrName];
		}

		public static ISelector Regex(string expr, int group)
		{
			if (!_cache.ContainsKey(expr))
			{
				_cache.Add(expr, new RegexSelector(expr, group));
			}
			return _cache[expr];
		}

		public static ISelector SmartContent()
		{
			return _cache["SmartContentSelector"];
		}

		public static ISelector XPath(string expr)
		{
			if (!_cache.ContainsKey(expr))
			{
				_cache.Add(expr, new XPathSelector(expr));
			}
			return _cache[expr];
		}

		//public static AndSelector And(params ISelector[] selectors)
		//{
		//	return new AndSelector(selectors);
		//}

		//public static OrSelector Or(params ISelector[] selectors)
		//{
		//	return new OrSelector(selectors);
		//}
	}
}