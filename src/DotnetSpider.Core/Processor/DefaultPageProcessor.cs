namespace DotnetSpider.Core.Processor
{
	public class DefaultPageProcessor : BasePageProcessor
	{
		public DefaultPageProcessor(string[] partterns = null, string[] excludeParterns = null)
		{
			var targetUrlsExtractor = new RegionAndPatternTargetUrlsExtractor();
			if (partterns != null && partterns.Length > 0)
			{
				targetUrlsExtractor.AddTargetUrlExtractor(".", partterns);
			}
			if (excludeParterns != null && excludeParterns.Length > 0)
			{
				targetUrlsExtractor.AddExcludeTargetUrlPatterns(excludeParterns);
			}
			TargetUrlsExtractor = targetUrlsExtractor;
		}

		public void AddTargetUrlExtractor(string regionXpath, params string[] patterns)
		{
			(TargetUrlsExtractor as RegionAndPatternTargetUrlsExtractor).AddTargetUrlExtractor(regionXpath, patterns);
		}

		protected override void Handle(Page page)
		{
			page.AddResultItem("title", page.Selectable.XPath("//title").GetValue());
			page.AddResultItem("html", page.Content);
			page.AddResultItem("url", page.Url);
		}
	}
}
