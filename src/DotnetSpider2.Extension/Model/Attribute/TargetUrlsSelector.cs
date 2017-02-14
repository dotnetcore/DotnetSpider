using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TargetUrlsSelector : System.Attribute
	{
		public string[] XPaths { get; set; } = new string[0];
		public string[] Patterns { get; set; } = new string[0];
	}
}
