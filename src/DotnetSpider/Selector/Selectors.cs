using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotnetSpider.Selector
{
	/// <summary>
	/// 查询器的构建帮助类, 相同的查询器会缓存起来.
	/// </summary>
	public class Selectors
	{
		private static readonly Dictionary<string, ISelector> Cache = new Dictionary<string, ISelector>();
		private static readonly EmptySelector DefaultSelector = new EmptySelector();

		/// <summary>
		/// 创建正则查询器
		/// </summary>
		/// <param name="expr">正则表达式</param>
		/// <returns>查询器</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static ISelector Regex(string expr)
		{
			var key = $"r_{expr}";
			if (!Cache.ContainsKey(key))
			{
				Cache.Add(key, new RegexSelector(expr));
			}
			return Cache[key];
		}

		/// <summary>
		/// 创建正则查询器
		/// </summary>
		/// <param name="expr">正则表达式</param>
		/// <param name="group"></param>
		/// <returns>查询器</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static ISelector Regex(string expr, int group)
		{
			var key = $"r_{expr}_{group}";
			if (!Cache.ContainsKey(key))
			{
				Cache.Add(key, new RegexSelector(expr, group));
			}
			return Cache[key];
		}

		/// <summary>
		/// 创建Css查询器
		/// </summary>
		/// <param name="expr">Css表达式</param>
		/// <returns>查询器</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static ISelector Css(string expr)
		{
			var key = $"c_{expr}";
			if (!Cache.ContainsKey(key))
			{
				Cache.Add(key, new CssSelector(expr));
			}
			return Cache[key];
		}

		/// <summary>
		/// 创建Css查询器
		/// </summary>
		/// <param name="expr">Css表达式</param>
		/// <param name="attrName">属性名称</param>
		/// <returns>查询器</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static ISelector Css(string expr, string attrName)
		{
			var key = $"c_{expr}_{attrName}";
			if (!Cache.ContainsKey(key))
			{
				Cache.Add(key, new CssSelector(expr, attrName));
			}
			return Cache[key];
		}

		/// <summary>
		/// 创建XPath查询器
		/// </summary>
		/// <param name="expr">Xpath表达式</param>
		/// <returns>查询器</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static ISelector XPath(string expr)
		{
			var key = $"x_{expr}";
			if (!Cache.ContainsKey(key))
			{
				Cache.Add(key, new XPathSelector(expr));
			}
			return Cache[key];
		}

		/// <summary>
		///  创建空查询器
		/// </summary>
		/// <returns>查询器</returns>
		public static ISelector Default()
		{
			return DefaultSelector;
		}

		/// <summary>
		/// 创建JsonPath查询器
		/// </summary>
		/// <param name="expr">JsonPath表达式</param>
		/// <returns>查询器</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static ISelector JsonPath(string expr)
		{
			var key = $"j_{expr}";
			if (!Cache.ContainsKey(key))
			{
				Cache.Add(key, new JsonPathSelector(expr));
			}
			return Cache[key];
		}
	}
}