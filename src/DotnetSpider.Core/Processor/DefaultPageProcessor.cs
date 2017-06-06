namespace DotnetSpider.Core.Processor
{
	public sealed class DefaultPageProcessor : BasePageProcessor
	{
		public DefaultPageProcessor(string[] partterns, string[] excludeParterns = null)
		{
			AddTargetUrlExtractor(".", partterns);
			AddExcludeTargetUrlPattern(excludeParterns);
		}

		protected override void Handle(Page page)
		{
			page.AddResultItem("title", page.Selectable.XPath("//title").GetValue());
			page.AddResultItem("html", page.Content);
		}
	}
}
