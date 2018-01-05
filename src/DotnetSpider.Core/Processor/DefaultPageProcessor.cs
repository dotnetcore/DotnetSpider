namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// Ĭ�Ͻ�����, û���ر�������, ���ڲ��Ե�
	/// </summary>
	public class DefaultPageProcessor : BasePageProcessor
	{
		/// <summary>
		/// ���췽��
		/// </summary>
		/// <param name="partterns">ƥ��Ŀ�����ӵ�������ʽ</param>
		/// <param name="excludeParterns">�ų�Ŀ�����ӵ�������ʽ</param>
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

		/// <summary>
		/// ���Ŀ�����ӽ�������
		/// </summary>
		/// <param name="regionXpath">Ŀ��������������</param>
		/// <param name="patterns">ƥ��Ŀ�����ӵ�������ʽ</param>
		public void AddTargetUrlExtractor(string regionXpath, params string[] patterns)
		{
			(TargetUrlsExtractor as RegionAndPatternTargetUrlsExtractor)?.AddTargetUrlExtractor(regionXpath, patterns);
		}

		/// <summary>
		/// ����ҳ������
		/// </summary>
		/// <param name="page">ҳ������</param>
		protected override void Handle(Page page)
		{
			page.AddResultItem("title", page.Selectable.XPath("//title").GetValue());
			page.AddResultItem("html", page.Content);
			page.AddResultItem("url", page.Url);
		}
	}
}
