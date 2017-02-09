using System.Collections.Generic;

namespace DotnetSpider.Core.Selector
{
	/// <summary>
	/// Selector(extractor) for text.
	/// </summary>
	public interface ISelector
	{
		/// <summary>
		/// Extract single result in text. 
		/// If there are more than one result, only the first will be chosen.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		dynamic Select(dynamic text);

		/// <summary>
		/// Extract all results in text.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		List<dynamic> SelectList(dynamic text);
	}
}
