using System;

namespace DotnetSpider.Core.Infrastructure
{
	[AttributeUsage(AttributeTargets.Class)]
	public class Properties : Attribute
	{
		public Properties(string Designer, string Developer, string Date, string Detail = null)
		{
			this.Designer = Designer;
			this.Developer = Developer;
			this.Detail = Detail;
			this.Date = Date;
		}

		public string Designer { get; set; }
		public string Developer { get; set; }
		public string Date { get; set; }
		public string Detail { get; set; }
	}
}
