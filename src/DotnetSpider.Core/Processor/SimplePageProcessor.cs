namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// A simple PageProcessor.
	/// </summary>
	public class SimplePageProcessor : BasePageProcessor
	{
		protected override void Handle(Page page)
		{
			page.AddResultItem("title", page.Selectable.XPath("//title"));
			page.AddResultItem("html", page.Content);
		}
	}
}
