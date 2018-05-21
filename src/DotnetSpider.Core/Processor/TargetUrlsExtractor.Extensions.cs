using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Selector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#if !NET45
using System.Net;
#endif

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 指定区域下的链接并且需要符合给定正则的链接为符合要求的目标链接
	/// </summary>
	public sealed class RegionAndPatternTargetUrlsExtractor : TargetUrlsExtractor
	{
		private readonly Dictionary<ISelector, List<Regex>> _regionSelectorMapPatterns = new Dictionary<ISelector, List<Regex>>();

		private static readonly ISelector ImageSelector = Selectors.XPath(".//img/@src");

		/// <summary>
		/// 构造方法
		/// </summary>
		public RegionAndPatternTargetUrlsExtractor()
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="regionXpath">目标链接所在区域</param>
		/// <param name="patterns">目标链接必须匹配的正则表达式</param>
		public RegionAndPatternTargetUrlsExtractor(string regionXpath, params string[] patterns)
		{
			AddTargetUrlExtractor(regionXpath, patterns);
		}

		/// <summary>
		/// 解析出目标链接
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <param name="site">站点信息</param>
		/// <returns>目标链接</returns>
		protected override IEnumerable<Request> Extract(Page page, Site site)
		{
			if (_regionSelectorMapPatterns == null || _regionSelectorMapPatterns.Count == 0)
			{
				return new Request[0];
			}

			List<string> resultUrls = new List<string>();
			foreach (var targetUrlExtractor in _regionSelectorMapPatterns)
			{
				if (Equals(targetUrlExtractor.Key, Selectors.Default()))
				{
					continue;
				}
				IEnumerable<string> links = null;
				if (page.ContentType == ContentType.Html)
				{
					links = page.Selectable.SelectList(targetUrlExtractor.Key).Links().GetValues();
				}
				else if (page.ContentType == ContentType.Json)
				{
					links = page.Selectable.SelectList(Selectors.Regex(RegexUtil.Url)).Links().GetValues();
				}
				else
				{
				}

				if (links == null)
				{
					continue;
				}

				// check: 仔细考虑是放在前面, 还是在后面做 formatter, 我倾向于在前面. 对targetUrl做formatter则表示Start Url也应该是要符合这个规则的。
				List<string> tmp = new List<string>();
				foreach (string link in links)
				{
					var newUrl = FormateUrl(link);
#if NET45
					tmp.Add(System.Web.HttpUtility.HtmlDecode(System.Web.HttpUtility.UrlDecode(newUrl)));
#else
					tmp.Add(WebUtility.HtmlDecode(WebUtility.UrlDecode(newUrl)));
#endif
				}
				links = tmp;

				if (targetUrlExtractor.Value == null || targetUrlExtractor.Value.Count == 0)
				{
					resultUrls.AddRange(links);
					continue;
				}

				foreach (var regex in targetUrlExtractor.Value)
				{
					foreach (string link in links)
					{
						if (regex.IsMatch(link))
						{
							bool isRequired = true;
							if (ExcludeTargetUrlPatterns != null)
							{
								foreach (var excludeRegex in ExcludeTargetUrlPatterns)
								{
									if (excludeRegex.IsMatch(link))
									{
										isRequired = false;
										break;
									}
								}
							}
							if (isRequired)
							{
								resultUrls.Add(link);
							}
						}
					}
				}
			}

			if (site.DownloadFiles)
			{
				var links = (page.Selectable.SelectList(ImageSelector)).GetValues();

				if (links != null && links.Count() > 0)
				{
					foreach (string link in links)
					{
						bool isRequired = true;
						if (ExcludeTargetUrlPatterns != null)
						{
							foreach (var excludeRegex in ExcludeTargetUrlPatterns)
							{
								if (excludeRegex.IsMatch(link))
								{
									isRequired = false;
									break;
								}
							}
						}
						if (isRequired)
						{
							resultUrls.Add(link);
						}
					}
				}

			}

			return resultUrls.Select(t => new Request(t, page.Request.Extras) { Site = site });
		}

		/// <summary>
		/// 添加目标链接解析规则
		/// </summary>
		/// <param name="regionXpath">目标链接所在区域</param>
		/// <param name="patterns">匹配目标链接的正则表达式</param>
		public void AddTargetUrlExtractor(string regionXpath, params string[] patterns)
		{
			if (patterns == null || patterns.Length == 0)
			{
				throw new ArgumentNullException("Patterns should not be null or empty.");
			}

			var validPatterns = patterns.Where(p => p != null && !string.IsNullOrWhiteSpace(p.Trim())).Select(p => p.Trim()).ToList();

			if (validPatterns.Count != patterns.Length)
			{
				throw new ArgumentNullException("Pattern value should not be null or empty.");
			}

			ISelector selector = Selectors.Regex(RegexUtil.Url);
			if (!string.IsNullOrWhiteSpace(regionXpath))
			{
				string xpath = string.IsNullOrWhiteSpace(regionXpath.Trim()) ? "." : regionXpath.Trim();
				selector = Selectors.XPath(xpath);
			}

			if (!_regionSelectorMapPatterns.ContainsKey(selector))
			{
				_regionSelectorMapPatterns.Add(selector, new List<Regex>());
			}
			var oldPatterns = _regionSelectorMapPatterns[selector];
			// 如果已经有正则为空, 即表示当前区域内所有的URL都是目标链接, 则无需再校验其它正则了
			if (oldPatterns.Contains(null))
			{
				return;
			}
			// 如果不提供正则表达式, 表示当前区域内所有的URL都是目标链接
			if (validPatterns.Count == 0)
			{
				oldPatterns.Add(null);
			}
			foreach (var pattern in validPatterns)
			{
				if (oldPatterns.All(p => p.ToString() != pattern))
				{
					oldPatterns.Add(new Regex(pattern));
					AddTargetUrlPatterns(pattern);
				}
			}
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		internal bool ContainsTargetUrlRegion(string region)
		{
			ISelector selector = Selectors.Default();
			if (!string.IsNullOrWhiteSpace(region))
			{
				selector = Selectors.XPath(region);
			}
			return _regionSelectorMapPatterns.ContainsKey(selector);
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="regionXpath"></param>
		/// <returns></returns>
		internal List<Regex> GetTargetUrlPatterns(string regionXpath)
		{
			ISelector selector = Selectors.Regex(RegexUtil.Url);
			if (!string.IsNullOrWhiteSpace(regionXpath))
			{
				selector = Selectors.XPath(regionXpath);
			}

			return _regionSelectorMapPatterns.ContainsKey(selector) ? _regionSelectorMapPatterns[selector] : null;
		}

		/// <summary>
		/// 自定义格式化链接
		/// </summary>
		/// <param name="url">目标链接</param>
		/// <returns>格式化后的链接</returns>
		private string FormateUrl(string url)
		{
			return url;
		}
	}

	/// <summary>
	/// 使用分页信息进行解析目标链接
	/// </summary>
	public abstract class PaginationTargetUrlsExtractor : TargetUrlsExtractor
	{
		/// <summary>
		/// 分页信息的正则表达式
		/// </summary>
		public readonly Regex PaginationPattern;

		/// <summary>
		/// http://a.com?p=40 PaginationStr: p=40 => Pattern: p=\d+
		/// </summary>
		public readonly string PaginationStr;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="paginationStr">分页信息片段： http://a.com?p=40 PaginationStr: p=40</param>
		/// <param name="termination">中止器</param>
		protected PaginationTargetUrlsExtractor(string paginationStr, ITargetUrlsExtractorTermination termination = null)
		{
			if (string.IsNullOrWhiteSpace(paginationStr))
			{
				throw new SpiderException("paginationStr should not be null or empty");
			}

			PaginationStr = paginationStr;
			PaginationPattern = new Regex($"{RegexUtil.Number.Replace(PaginationStr, @"\d+")}");
			TargetUrlsExtractorTermination = termination;
		}

		/// <summary>
		/// 取得当前分页
		/// </summary>
		/// <param name="currentUrlOrContent">当前链接或者内容(有的分页信息放在Cookie或者Post的内容里)</param>
		/// <returns></returns>
		protected string GetCurrentPagination(string currentUrlOrContent)
		{
			return PaginationPattern.Match(currentUrlOrContent).Value;
		}
	}

	/// <summary>
	/// 通过自增计算出新的目标链接, 比如: www.a.com/1.html->www.a.com/2.html
	/// </summary>
	public class AutoIncrementTargetUrlsExtractor : PaginationTargetUrlsExtractor
	{
		private readonly int _interval;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="paginationStr">URL中分页的部分, 如: www.a.com/content_1.html, 则可以填此值为 content_1.html, tent_1.html等, 框架会把数据部分改成\d+用于正则匹配截取</param>
		/// <param name="interval">每次自增的间隔</param>
		/// <param name="termination">中止器, 用于判断是否已到最后一个需要采集的链接</param>
		public AutoIncrementTargetUrlsExtractor(string paginationStr, int interval = 1, ITargetUrlsExtractorTermination termination = null) : base(paginationStr, termination)
		{
			_interval = interval;
		}

		/// <summary>
		/// 解析出目标链接
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <param name="site">站点信息</param>
		/// <returns>目标链接</returns>
		protected override IEnumerable<Request> Extract(Page page, Site site)
		{
			var currentPageStr = GetCurrentPagination(page.Request.Url);
			var matches = RegexUtil.Number.Matches(currentPageStr);
			if (matches.Count > 0 && int.TryParse(matches[0].Value, out var currentPage))
			{
				var next = RegexUtil.Number.Replace(PaginationStr, (currentPage + _interval).ToString());
				string newUrl = page.Request.Url.Replace(currentPageStr, next);
				return new Request[] { new Request(newUrl, page.Request.Extras) { Site = site } };
			}

			return new Request[0];
		}
	}
}