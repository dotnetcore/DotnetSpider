namespace Java2Dotnet.Spider.Core.Processor
{
	/// <summary>
	/// Interface to be implemented to customize a crawler. 
	/// In PageProcessor, you can customize:
	/// start urls and other settings in {@link Site} 
	/// how the urls to fetch are detected               
	/// how the data are extracted and stored            
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
		Site Site { get; }
	}
}
