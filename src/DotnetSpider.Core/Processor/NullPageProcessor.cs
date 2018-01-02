using System;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 空解析器, 仅用于容错
	/// </summary>
	public class NullPageProcessor : BasePageProcessor
	{
		protected override void Handle(Page page)
		{
			Console.WriteLine("You used a null processor.");
		}
	}
}
