namespace DotnetSpider.Core.Processor
{
	public sealed class DefaultPageProcessor : BasePageProcessor
	{

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="partterns">匹配目标链接的正则表达式</param>
        /// <param name="excludeParterns">排除目标链接的正则表达式</param>
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

    

        /// <summary>
        /// 解析页面数据
        /// </summary>
        /// <param name="page">页面数据</param>
        protected override void Handle(Page page)
		{
			page.AddResultItem("title", page.Selectable.XPath("//title").GetValue());
			page.AddResultItem("html", page.Content);
			page.AddResultItem("url", page.Url);
		}
	}
}
