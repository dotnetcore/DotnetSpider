namespace DotnetSpider.Core.Processor
{
	public class DefaultPageProcessor : BasePageProcessor
	{
		protected override void Handle(Page page)
		{
			page.AddResultItem("Html", page.Content);
		}
	}
}
