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
		private List<Regex> _targetUrlPatterns;
		private readonly List<Regex> _excludeTargetUrlPatterns = new List<Regex>();
		private readonly Dictionary<ISelector, List<Regex>> _targetUrlExtractors = new Dictionary<ISelector, List<Regex>>();
		private readonly ISelector _imageSelector = Selectors.XPath(".//img/@src");

		protected abstract void Handle(Page page);

		public void Process(Page page)
		{
			if (_targetUrlExtractors != null)
			{
				bool isTarget = true;

				foreach (var regex in GetTargetUrlPatterns())
				{
					isTarget = regex.IsMatch(page.Url);
					if (isTarget)
					{
						break;
					}
				}
				if (!isTarget)
				{
					return;
				}
			}

			Handle(page);

			page.ResultItems.IsSkip = page.ResultItems.Results.Count == 0;

			if (!page.MissExtractTargetUrls)
			{
				ExtractUrls(page);
			}
		}

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

		protected virtual List<Regex> GetTargetUrlPatterns()
		{
			if (_targetUrlPatterns == null)
			{
				_targetUrlPatterns = new List<Regex>();
				foreach (var targetUrlExtractor in _targetUrlExtractors)
				{
					foreach (var pattern in targetUrlExtractor.Value)
					{
						_targetUrlPatterns.Add(pattern);
					}
				}
			}
			return _targetUrlPatterns;
		}

		protected virtual void AddTargetUrlExtractor(string regionXpath, params string[] patterns)
		{
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

			if (patterns == null || patterns.Length == 0)
			{
				if (!realPatterns.Contains(null))
				{
					realPatterns.Add(null);
				}
				return;
			}
			foreach (var pattern in patterns)
			{
				var realPattern = string.IsNullOrEmpty(pattern?.Trim()) ? null : pattern.Trim();
				if (realPatterns.All(p => p.ToString() != realPattern))
				{
					realPatterns.Add(new Regex(realPattern));
				}
			}
		}

		protected virtual void AddExcludeTargetUrlPattern(params string[] patterns)
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

		/// <summary>
		/// Only used for test
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		[Obsolete]
		public virtual List<Regex> GetTargetUrlPatterns(string region)
		{
			ISelector selector = Selectors.Default();
			if (!string.IsNullOrWhiteSpace(region))
			{
				selector = Selectors.XPath(region);
			}

			if (_targetUrlExtractors.ContainsKey(selector))
			{
				return _targetUrlExtractors[selector];
			}
			else
			{
				return null;
			}
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
	}
}
