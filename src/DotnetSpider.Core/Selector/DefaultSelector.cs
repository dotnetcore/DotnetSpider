using System.Collections.Generic;

namespace DotnetSpider.Core.Selector
{
	/// <summary>
	/// 空的选择器
	/// </summary>
	internal class DefaultSelector : ISelector
	{
		/// <summary>
		/// 从文本中查询单个结果
		/// 如果符合条件的结果有多个, 仅返回第一个
		/// </summary>
		/// <param name="text">需要查询的文本</param>
		/// <returns>查询结果</returns>
		public dynamic Select(dynamic text)
		{
			return null;
		}

		/// <summary>
		/// 从文本中查询所有结果
		/// </summary>
		/// <param name="text">需要查询的文本</param>
		/// <returns>查询结果</returns>
		public List<dynamic> SelectList(dynamic text)
		{
			return null;
		}
	}
}
