using DotnetSpider.Core.Infrastructure;
using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	/// <summary>
	/// 目标链接选择器的定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class TargetUrlsSelector : System.Attribute
	{
		public TargetUrlsSelector() { }

		public TargetUrlsSelector(string[] xpaths, string[] patterns = null)
		{
			XPaths = xpaths;
			Patterns = patterns;
		}

		public TargetUrlsSelector(string xpath)
		{
			XPaths = new[] { xpath };
			Patterns = new[] { RegexUtil.Url };
		}

		public TargetUrlsSelector(string xpath, string pattern)
		{
			XPaths = new[] { xpath };
			Patterns = new[] { pattern };
		}

		/// <summary>
		/// 目标链接所在区域
		/// </summary>
		public string[] XPaths { get; set; }

		/// <summary>
		/// 匹配目标链接的正则表达式
		/// </summary>
		public string[] Patterns { get; set; }
	}
}
