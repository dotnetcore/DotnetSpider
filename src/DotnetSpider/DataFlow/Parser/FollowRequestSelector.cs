using System;
using DotnetSpider.Selector;
using Newtonsoft.Json;

namespace DotnetSpider.DataFlow.Parser
{
	/// <summary>
	/// 目标链接选择器的定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class FollowRequestSelector : Attribute
	{
		/// <summary>
		/// 查询器类型
		/// </summary>
		public SelectorType SelectorType { get; set; } = SelectorType.XPath;

		/// <summary>
		/// 查询表达式
		/// </summary>
		public string[] Expressions { get; set; }

#if !NET451
		/// <summary>
		/// 避免被序列化出去
		/// </summary>
		[JsonIgnore]
		public override object TypeId => base.TypeId;
#endif

		/// <summary>
		/// 匹配目标链接的正则表达式
		/// </summary>
		public string[] Patterns { get; set; } = new string[0];
	}
}
