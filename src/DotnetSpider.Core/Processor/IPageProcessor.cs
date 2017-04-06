namespace DotnetSpider.Core.Processor
{
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
