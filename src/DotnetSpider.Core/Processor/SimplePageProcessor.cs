namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// A simple PageProcessor.
	/// </summary>
	public class SimplePageProcessor : BasePageProcessor
	{
		/// <summary>
		/// 解析出页面的title和html
		/// </summary>
		/// <param name="page">页面数据</param>
		protected override void Handle(Page page)
		{
			page.AddResultItem("html", page.Content);
		}
	}
}
