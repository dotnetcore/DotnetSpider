using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Class)]
	public class EntitySelector : BaseSelector
	{
		public int Take { get; set; } = -1;
	}
}
