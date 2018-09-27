using Newtonsoft.Json;
using System;

namespace DotnetSpider.Extraction.Model.Attribute
{
	/// <summary>
	/// 目标链接选择器的定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class Target : System.Attribute
	{
		[JsonIgnore]
		public override object TypeId => base.TypeId;

		public Target() { }

		public Target(string[] xpaths, string[] patterns = null)
		{
			XPaths = xpaths;
			Patterns = patterns;
		}

		public Target(string xpath)
		{
			XPaths = new[] { xpath };
		}

		public Target(string xpath, string pattern)
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

		/// <summary>
		/// 需要排除链接的正则表达式
		/// </summary>
		public string[] ExcludePatterns { get; set; }
	}
}
