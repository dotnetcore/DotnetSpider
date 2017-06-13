using DotnetSpider.Extension;
using DotnetSpider.Extension.Infrastructure;
using System;

namespace DotnetSpider.Sample
{
	public class CustomSpider1 : CustomSpider
	{
		public CustomSpider1() : base("CustomSpider1", Extension.Infrastructure.Batch.Daily)
		{
		}

		protected override void ImplementAction(params string[] arguments)
		{
			Console.WriteLine("hello");
		}
	}
}
