using DotnetSpider.Core.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 如果包含指定内容则到了最后一个采集链接
	/// </summary>
	public class ContainsTermination : ITargetUrlsExtractorTermination
	{
		private readonly string[] _contains;

		public ContainsTermination(string[] contains)
		{
			_contains = contains;
		}

		public bool IsTermination(Page page)
		{
			if (string.IsNullOrEmpty(page?.Content))
			{
				return false;
			}

			return _contains.Any(c => page.Content.Contains(c));
		}
	}

	/// <summary>
	/// 如果不包含指定内容则到了最后一个采集链接
	/// </summary>
	public class UnContainsTermination : ITargetUrlsExtractorTermination
	{
		private readonly string[] _unContains;

		public UnContainsTermination(string[] unContains)
		{
			_unContains = unContains;
		}

		public bool IsTermination(Page page)
		{
			if (string.IsNullOrEmpty(page?.Content))
			{
				return false;
			}

			return !_unContains.All(c => page.Content.Contains(c));
		}
	}

	/// <summary>
	/// 最大分页数限制
	/// </summary>
	public class MaxPageTermination : ITargetUrlsExtractorTermination
	{
		private readonly Regex _paginationPattern;
		private readonly int _maxPage;

		public MaxPageTermination(string paginationStr, int maxPage)
		{
			if (string.IsNullOrEmpty(paginationStr) || string.IsNullOrWhiteSpace(paginationStr))
			{
				throw new SpiderException("paginationStr should not be null.");
			}
			_paginationPattern = new Regex($"{RegexUtil.Number.Replace(paginationStr, @"\d+")}");
			_maxPage = maxPage;
		}

		public bool IsTermination(Page page)
		{
			var currentPage = int.Parse(_paginationPattern.Match(page.Url).Value);
			return currentPage < _maxPage;
		}
	}
}
