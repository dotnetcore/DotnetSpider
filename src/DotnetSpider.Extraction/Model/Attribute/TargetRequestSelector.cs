using System;

namespace DotnetSpider.Extraction.Model.Attribute
{
	/// <summary>
	/// 目标链接选择器的定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class TargetRequestSelector : System.Attribute
	{
		public TargetRequestSelector() { }

		public TargetRequestSelector(string[] xpaths, string[] patterns = null)
		{
			XPaths = xpaths;
			Patterns = patterns;
		}

		public TargetRequestSelector(string xpath)
		{
			XPaths = new[] { xpath };
			Patterns = new[] { @"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?" };
		}

		public TargetRequestSelector(string xpath, string pattern)
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
