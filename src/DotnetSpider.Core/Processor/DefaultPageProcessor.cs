using DotnetSpider.Core.Processor.TargetRequestExtractors;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 默认解析器, 没有特别大的作用, 用于测试等
	/// </summary>
	public class DefaultPageProcessor : BasePageProcessor
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="partterns">匹配目标链接的正则表达式</param>
		/// <param name="excludeParterns">排除目标链接的正则表达式</param>
		public DefaultPageProcessor(string[] partterns = null, string[] excludeParterns = null)
		{
			var targetUrlsExtractor = new RegionAndPatternTargetRequestExtractor();
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

		/// <summary>
		/// 添加目标链接解析规则
		/// </summary>
		/// <param name="regionXpath">目标链接所在区域</param>
		/// <param name="patterns">匹配目标链接的正则表达式</param>
		public void AddTargetUrlExtractor(string regionXpath, params string[] patterns)
		{
			(TargetUrlsExtractor as RegionAndPatternTargetRequestExtractor)?.AddTargetUrlExtractor(regionXpath, patterns);
		}

		/// <summary>
		/// 解析页面数据
		/// </summary>
		/// <param name="page">页面数据</param>
		protected override void Handle(Page page)
		{
			page.AddResultItem("title", page.Selectable().XPath("//title").GetValue());
			page.AddResultItem("html", page.Content);
			page.AddResultItem("url", page.Request.Url);
		}
	}

	class Item
	{
		public string Title { get; set; }
	}
}
