using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	/// <summary>
	/// 目标链接选择器的定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class TargetUrlsSelector : System.Attribute
	{
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
