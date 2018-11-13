using System.Linq;

namespace DotnetSpider.Core.Processor.LastPageChecker
{
	/// <summary>
	/// 如果不包含指定内容则到了最后一个采集链接
	/// </summary>
	public class UnContainsLastPageChecker : ILastPageChecker
	{
		private readonly string[] _unContains;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="unContains">不包含的内容</param>
		public UnContainsLastPageChecker(string[] unContains)
		{
			_unContains = unContains;
		}

		/// <summary>
		/// 是否到了最后一个链接
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <returns>如果返回 True, 则说明已经采到到了最后一个链接</returns>
		public bool IsLastPage(Page page)
		{
			var text = page?.Content?.ToString();
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}

			return !_unContains.All(c => text.Contains(c));
		}
	}
}
