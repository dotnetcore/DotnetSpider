using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class GlobalValueSelector : BaseSelector
	{
		public string Name { get; set; }
	}
}
