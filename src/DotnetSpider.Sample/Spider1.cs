using DotnetSpider.Extension;
using DotnetSpider.Extension.Infrastructure;
using System;

namespace DotnetSpider.Sample
{
	public class Spider1 : CustomSpider
	{
		public Spider1() : base("CustomSpider1", Extension.Infrastructure.Batch.Daily)
		{
		}

		protected override void ImplementAction(params string[] arguments)
		{
			Console.WriteLine("hello");
		}
	}
}
