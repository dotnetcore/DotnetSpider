using DotnetSpider.Extension;
using System;

namespace DotnetSpider.Sample
{
	public class CustomSpider1 : CustomSpider
	{
		public CustomSpider1() : base("CustomSpider1")
		{
			Identity = Guid.NewGuid().ToString();
		}

		protected override void ImplementAction(params string[] arguments)
		{
			Console.WriteLine("hello");
		}
	}
}

