using System.Linq;

namespace DotnetSpider.Core.Processor.LastPageChecker
{
	/// <summary>
	/// 如果包含指定内容则到了最后一个采集链接
	/// </summary>
	public class ContainsLastPageChecker : ILastPageChecker
	{
		private readonly string[] _contains;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="contains">包含的内容</param>
		public ContainsLastPageChecker(string[] contains)
		{
			_contains = contains;
		}

		/// <summary>
		/// 是否到了最后一个链接
		/// </summary>
		/// <param name="response">链接请求结果</param>
		/// <returns>是否到了最终一个链接</returns>
		public bool IsLastPage(Page page)
		{
			var text = page?.Content == null ? "" : page.Content.ToString();

			return _contains.Any(c => text.Contains(c));
		}
	}
}
