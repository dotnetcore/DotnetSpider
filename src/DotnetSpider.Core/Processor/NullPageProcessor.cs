using System;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 空解析器, 仅用于容错
	/// </summary>
	public class NullPageProcessor : BasePageProcessor
	{
		/// <summary>
		/// 不做任何解析操作
		/// </summary>
		/// <param name="page">页面数据</param>
		protected override void Handle(Page page)
		{
			Console.WriteLine("You used a null processor.");
		}
	}
}
