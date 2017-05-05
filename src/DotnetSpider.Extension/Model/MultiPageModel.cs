using System.Collections.Generic;

namespace DotnetSpider.Extension.Model
{
	public interface IMultiPageModel
	{
		/// <summary>
		/// Page key is the identifier for the object.
		/// </summary>
		/// <returns></returns>
		string GetPageKey();

		/// <summary>
		/// Page is the identifier of a page in pages for one object.
		/// </summary>
		/// <returns></returns>
		string GetPage();

		/// <summary>
		/// Other pages to be extracted.
		/// It is used to judge whether an object contains more than one page, and whether the pages of the object are all extracted
		/// </summary>
		/// <returns></returns>
		ICollection<string> GetOtherPages();

		/// <summary>
		/// Combine multiPageModels to a whole object.
		/// </summary>
		/// <param name="multiPageModel"></param>
		/// <returns></returns>
		IMultiPageModel Combine(IMultiPageModel multiPageModel);
	}
}
