using System.Collections.Generic;

namespace DotnetSpider.Core.Selector
{
	/// <summary>
	/// Selectable text.
	/// </summary>
	public interface ISelectable 
	{
		/// <summary>
		/// Select list with xpath
		/// </summary>
		/// <param name="xpath"></param>
		/// <returns></returns>
		ISelectable XPath(string xpath);

		/// <summary>
		/// Select list with css selector
		/// </summary>
		/// <param name="selector"></param>
		/// <returns></returns>
		ISelectable Css(string selector);

		/// <summary>
		/// Select list with css selector
		/// </summary>
		/// <param name="selector"></param>
		/// <param name="attrName"></param>
		/// <returns></returns>
		ISelectable Css(string selector, string attrName);

		/// <summary>
		/// Select smart content with ReadAbility algorithm
		/// </summary>
		/// <returns></returns>
		ISelectable SmartContent();

		/// <summary>
		/// Select all links
		/// </summary>
		/// <returns></returns>
		ISelectable Links();

		/// <summary>
		/// Get all nodes
		/// </summary>
		/// <returns></returns>
		IList<ISelectable> Nodes();

		ISelectable JsonPath(string path);

		/// <summary>
		/// Select list with regex, default group is group 1
		/// </summary>
		/// <param name="regex"></param>
		/// <returns></returns>
		ISelectable Regex(string regex);

		/// <summary>
		/// Select list with regex
		/// </summary>
		/// <param name="regex"></param>
		/// <param name="group"></param>
		/// <returns></returns>
		ISelectable Regex(string regex, int group);

		///// <summary>
		///// Replace with regex
		///// </summary>
		///// <param name="regex"></param>
		///// <param name="replacement"></param>
		///// <returns></returns>
		//ISelectable Replace(string regex, string replacement);

		/// <summary>
		/// Single string result
		/// </summary>
		string GetValue(bool isPlainText = false);

		List<string> GetValues(bool isPlainText = false);

		///// <summary>
		///// If result exist for select
		///// </summary>
		///// <returns></returns>
		////bool Exist();

		/// <summary>
		/// Extract by custom selector
		/// </summary>
		/// <param name="selector"></param>
		/// <returns></returns>
		ISelectable Select(ISelector selector);

		/// <summary>
		/// Extract by custom selector
		/// </summary>
		/// <param name="selector"></param>
		/// <returns></returns>
		ISelectable SelectList(ISelector selector);
	}
}
