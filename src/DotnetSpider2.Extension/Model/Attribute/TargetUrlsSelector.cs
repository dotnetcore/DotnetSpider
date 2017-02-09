using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TargetUrlsSelector : System.Attribute
	{
		public string[] XPaths { get; set; }
		public string[] Patterns { get; set; }
	}
}
