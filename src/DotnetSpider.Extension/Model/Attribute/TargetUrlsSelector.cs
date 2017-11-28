using System;

namespace DotnetSpider.Extension.Model.Attribute
{

    /// <summary>
    /// 分页选择器
    /// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class TargetUrlsSelector : System.Attribute
	{
		public string[] XPaths { get; set; }
		public string[] Patterns { get; set; }
	}
}
