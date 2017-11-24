namespace DotnetSpider.Core.Processor
{

    /// <summary>
    /// 负责HTML解析、目标URL的选择
    /// </summary>
	public interface IPageProcessor
	{
		/// <summary>
		/// Process the page, extract urls to fetch, extract the data and store
		/// </summary>
		/// <param name="page"></param>
		void Process(Page page);

		/// <summary>
		/// Get the site settings
		/// </summary>
		Site Site { get; set; }
	}
}
