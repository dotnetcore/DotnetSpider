namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// Use to test.
	/// </summary>
	public class DefaultPageProcessor : IPageProcessor
	{
		public void Process(Page page)
		{
		}

		/// <summary>
		/// Get the site settings
		/// </summary>
		public Site Site { get; set; }
	}
}
