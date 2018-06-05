namespace DotnetSpider.Core.Processor
{
	public sealed class DefaultPageProcessor : BasePageProcessor
	{

        /// <summary>
        /// ���췽��
        /// </summary>
        /// <param name="partterns">ƥ��Ŀ�����ӵ�������ʽ</param>
        /// <param name="excludeParterns">�ų�Ŀ�����ӵ�������ʽ</param>
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
