using System;

namespace DotnetSpider.Core.Processor
{
	public class NullPageProcessor : BasePageProcessor
	{
		protected override void Handle(Page page)
		{
			Console.WriteLine("You used a null processor.");
		}
	}
}
