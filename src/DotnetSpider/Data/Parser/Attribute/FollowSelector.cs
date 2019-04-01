using System;
using Newtonsoft.Json;

namespace DotnetSpider.Data.Parser.Attribute
{
	/// <summary>
	/// 目标链接选择器的定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class FollowSelector : System.Attribute
	{
#if !NET451
		/// <summary>
		/// 避免被序列化出去
		/// </summary>
		[JsonIgnore]
		public override object TypeId => base.TypeId;
#endif
		public FollowSelector()
		{
		}

		public FollowSelector(string[] xPaths, string[] patterns = null)
		{
			XPaths = xPaths;
			Patterns = patterns;
		}

		public FollowSelector(string xpath)
		{
			XPaths = new[] {xpath};
		}

		public FollowSelector(string xpath, string pattern)
		{
			XPaths = new[] {xpath};
			Patterns = new[] {pattern};
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