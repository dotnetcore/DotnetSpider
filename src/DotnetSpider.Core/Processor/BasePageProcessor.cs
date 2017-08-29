using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core.Selector;
using System.Linq;
using System;
#if !NET_CORE
using System.Web;
#else
using System.Net;
#endif

namespace DotnetSpider.Core.Processor
{
	public abstract class BasePageProcessor : IPageProcessor
	{
		private readonly static ISelector _imageSelector = Selectors.XPath(".//img/@src");

		private readonly List<Regex> _excludeTargetUrlPatterns = new List<Regex>();
		private readonly Dictionary<ISelector, List<Regex>> _targetUrlExtractors = new Dictionary<ISelector, List<Regex>>();
		private readonly HashSet<Regex> _targetUrlPatterns = new HashSet<Regex>();

		protected abstract void Handle(Page page);

		public void Process(Page page)
		{
			bool isTarget = true;

			if (_targetUrlPatterns.Count > 0 && !_targetUrlPatterns.Contains(null))
			{
				foreach (var regex in _targetUrlPatterns)
				{
					isTarget = regex.IsMatch(page.Url);
					if (isTarget)
					{
						break;
					}
				}
			}

			if (!isTarget)
			{
				return;
			}

			Handle(page);

			page.ResultItems.IsSkip = page.ResultItems.Results.Count == 0;

			if (!page.SkipExtractTargetUrls)
			{
				ExtractUrls(page);
			}
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="regionXpath"></param>
		/// <returns></returns>
		public virtual List<Regex> GetTargetUrlPatterns(string regionXpath)
		{
			ISelector selector = Selectors.Default();
			if (!string.IsNullOrWhiteSpace(regionXpath))
			{
				selector = Selectors.XPath(regionXpath);
			}

			return _targetUrlExtractors.ContainsKey(selector) ? _targetUrlExtractors[selector] : null;
		}

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		[Obsolete]
		public virtual bool ContainsTargetUrlRegion(string region)
		{
			ISelector selector = Selectors.Default();
			if (!string.IsNullOrWhiteSpace(region))
			{
				selector = Selectors.XPath(region);
			}
			return _targetUrlExtractors.ContainsKey(selector);
		}

		/// <summary>
		/// Get the site settings
		/// </summary>
		public Site Site { get; set; }

		/// <summary>
		/// 如果找不到则不返回URL, 不然返回的URL太多
		/// </summary>
		/// <param name="page"></param>
		protected virtual void ExtractUrls(Page page)
		{
			if (_targetUrlExtractors == null || _targetUrlExtractors.Count == 0)
			{
				return;
			}

			foreach (var targetUrlExtractor in _targetUrlExtractors)
			{
				if (Equals(targetUrlExtractor.Key, Selectors.Default()))
				{
					continue;
				}

				var links = page.Selectable.SelectList(targetUrlExtractor.Key).Links().GetValues();

				if (links == null)
				{
					continue;
				}

				// check: 仔细考虑是放在前面, 还是在后面做 formatter, 我倾向于在前面. 对targetUrl做formatter则表示Start Url也应该是要符合这个规则的。
				List<string> tmp = new List<string>();
				foreach (string link in links)
				{
					var url = FormateUrl(link);
#if !NET_CORE
					tmp.Add(HttpUtility.HtmlDecode(HttpUtility.UrlDecode(url)));
#else
					tmp.Add(WebUtility.HtmlDecode(WebUtility.UrlDecode(url)));
#endif
				}
				links = tmp;

				if (targetUrlExtractor.Value == null || targetUrlExtractor.Value.Count == 0)
				{
					page.AddTargetRequests(links);
					continue;
				}

				foreach (var regex in targetUrlExtractor.Value)
				{
					foreach (string link in links)
					{
						if (regex.IsMatch(link))
						{
							bool isRequired = true;
							if (_excludeTargetUrlPatterns != null)
							{
								foreach (var excludeRegex in _excludeTargetUrlPatterns)
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
								page.AddTargetRequest(new Request(link, page.Request.Extras));
							}
						}
					}
				}
			}

			if (Site.DownloadFiles)
			{
				var links = (page.Selectable.SelectList(_imageSelector)).GetValues();

				if (links == null || links.Count == 0)
				{
					return;
				}
				foreach (string link in links)
				{
					bool isRequired = true;
					if (_excludeTargetUrlPatterns != null)
					{
						foreach (var excludeRegex in _excludeTargetUrlPatterns)
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
						page.AddTargetRequest(new Request(link, page.Request.Extras));
					}
				}
			}
		}

		protected virtual string FormateUrl(string url)
		{
			return url;
		}

		protected void AddTargetUrlExtractor(string regionXpath, params string[] patterns)
		{
			var validPatterns = patterns.Where(p => p != null && !string.IsNullOrEmpty(p.Trim())).Select(p => p.Trim()).ToList();

			if (validPatterns.Count != patterns.Length)
			{
				throw new ArgumentNullException("Pattern should not be null or empty.");
			}

			ISelector selector = Selectors.Default();
			if (regionXpath != null)
			{
				string xpath = string.IsNullOrWhiteSpace(regionXpath.Trim()) ? "." : regionXpath.Trim();
				selector = Selectors.XPath(xpath);
			}
			if (!_targetUrlExtractors.ContainsKey(selector))
			{
				_targetUrlExtractors.Add(selector, new List<Regex>());
			}
			var realPatterns = _targetUrlExtractors[selector];
			// 如果已经有正则为空, 即表示当前区域内所有的URL都是目标链接, 则无需再校验其它正则了
			if (realPatterns.Contains(null))
			{
				return;
			}

			if (validPatterns.Count == 0)
			{
				if (!realPatterns.Contains(null))
				{
					realPatterns.Add(null);
				}
				return;
			}
			foreach (var pattern in validPatterns)
			{
				if (realPatterns.All(p => p.ToString() != pattern))
				{
					var regex = new Regex(pattern);
					realPatterns.Add(regex);
					_targetUrlPatterns.Add(regex);
				}
			}
		}

		protected void AddExcludeTargetUrlPattern(params string[] patterns)
		{
			if (patterns == null || patterns.Length == 0)
			{
				return;
			}
			foreach (var pattern in patterns)
			{
				if (_excludeTargetUrlPatterns.All(p => p.ToString() != pattern))
				{
					_excludeTargetUrlPatterns.Add(new Regex(pattern));
				}
			}
		}
	}
}
