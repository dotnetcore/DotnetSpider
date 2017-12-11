namespace DotnetSpider.Core.Processor
{
	public class DefaultPageProcessor : BasePageProcessor
	{
		public DefaultPageProcessor(string[] partterns = null, string[] excludeParterns = null)
		{
			if (partterns != null && partterns.Length > 0)
			{
				AddTargetUrlExtractor(".", partterns);
			}
			if (excludeParterns != null && excludeParterns.Length > 0)
			{
				AddExcludeTargetUrlPattern(excludeParterns);
			}
		}

		protected override void Handle(Page page)
		{
			page.AddResultItem("title", page.Selectable.XPath("//title").GetValue());
			page.AddResultItem("html", page.Content);
			page.AddResultItem("url", page.Url);
		}
	}
}
