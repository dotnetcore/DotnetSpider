using System;

namespace DotnetSpider.Core.Infrastructure
{
	[AttributeUsage(AttributeTargets.Class)]
	public class Properties : Attribute
	{
		public string Owner { get; set; }
		public string Developer { get; set; }
		public string Date { get; set; }
		public string Subject { get; set; }
		public string Email { get; set; }
	}
}
