using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	/// <summary>
	/// 当前属性的值(链接)是下一个爬虫的起始链接
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class ToNext : System.Attribute
	{
		/// <summary>
		/// 保存到起始链接的额外信息
		/// </summary>
		public string[] Extras { get; set; }

		/// <summary>
		/// 属性名称
		/// </summary>
		internal string PropertyName { get; set; }
	}
}
