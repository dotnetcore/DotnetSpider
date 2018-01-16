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

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="contains">包含的内容</param>
		public ContainsTermination(string[] contains)
		{
			_contains = contains;
		}

		/// <summary>
		/// 是否到了最后一个链接
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <returns>如果返回 True, 则说明已经采到到了最后一个链接</returns>
		public bool IsTermination(Page page)
		{
			if (page == null || string.IsNullOrWhiteSpace((page.Content)))
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

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="unContains">不包含的内容</param>
		public UnContainsTermination(string[] unContains)
		{
			_unContains = unContains;
		}

		/// <summary>
		/// 是否到了最后一个链接
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <returns>如果返回 True, 则说明已经采到到了最后一个链接</returns>
		public bool IsTermination(Page page)
		{
			if (page == null || string.IsNullOrWhiteSpace((page.Content)))
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

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="paginationStr">分页信息片段： http://a.com?p=40 PaginationStr: p=40</param>
		/// <param name="maxPage">最大分页数限制</param>
		public MaxPageTermination(string paginationStr, int maxPage)
		{
			if (string.IsNullOrWhiteSpace(paginationStr))
			{
				throw new SpiderException("paginationStr should not be null.");
			}
			_paginationPattern = new Regex($"{RegexUtil.Number.Replace(paginationStr, @"(?<page>\d+)")}");
			_maxPage = maxPage;
		}

		/// <summary>
		/// 是否到了最后一个链接
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <returns>如果返回 True, 则说明已经采到到了最后一个链接</returns>
		public bool IsTermination(Page page)
		{
			var currentPage = int.Parse(_paginationPattern.Match(page.Url).Groups["page"].Value);
			return currentPage >= _maxPage;
		}
	}
}
