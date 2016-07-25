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
			lock (_cache)
			{
				_cache.Add("SmartContentSelector", new SmartContentSelector());
			}
		}

		public static ISelector Regex(string expr)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(expr))
				{
					_cache.Add(expr, new RegexSelector(expr));
				}
				return _cache[expr];
			}
		}

		public static ISelector Css(string expr)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(expr))
				{
					_cache.Add(expr, new CssHtmlSelector(expr));
				}
				return _cache[expr];
			}
		}

		public static ISelector Css(string expr, string attrName)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(expr + attrName))
				{
					_cache.Add(expr + attrName, new CssHtmlSelector(expr, attrName));
				}
				return _cache[expr + attrName];
			}
		}

		public static ISelector Regex(string expr, int group)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(expr))
				{
					_cache.Add(expr, new RegexSelector(expr, group));
				}
				return _cache[expr];
			}
		}

		public static ISelector SmartContent()
		{
			lock (_cache)
			{
				return _cache["SmartContentSelector"];
			}
		}

		public static ISelector XPath(string expr)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(expr))
				{
					_cache.Add(expr, new XPathSelector(expr));
				}
				return _cache[expr];
			}
		}

		public static ISelector Enviroment(string expr)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(expr))
				{
					_cache.Add(expr, new EnviromentSelector(expr));
				}
				return _cache[expr];
			}
		}

		public static ISelector JsonPath(string expr)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(expr))
				{
					_cache.Add(expr, new JsonPathSelector(expr));
				}
				return _cache[expr];
			}
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