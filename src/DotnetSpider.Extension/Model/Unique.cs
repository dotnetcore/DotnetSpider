using System;

namespace DotnetSpider.Extension.Model
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class Unique : Attribute
	{
		public string Name { get; set; }

		public Unique()
		{
		}

		public Unique(string name)
		{
			Name = name;
		}
	}
}
