using System;

namespace DotnetSpider.Extension.Model
{
	[AttributeUsage(AttributeTargets.Property)]
	public class Index : Attribute
	{
		public string Name { get; set; }

		public Index()
		{
		}

		public Index(string name)
		{
			Name = name;
		}
	}
}
