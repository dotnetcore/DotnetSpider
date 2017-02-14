using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetSpider.Core.Selector;
using System.Linq;
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

		private Dictionary<ISelector, List<string>> _targetUrlExtractors { get; set; } = new Dictionary<ISelector, List<string>>();

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

			if (page.ResultItems.Results.Count == 0)
			{
				page.ResultItems.IsSkip = true;
			}
			else
			{
				page.ResultItems.IsSkip = false;
			}

			if (!page.MissExtractTargetUrls)
			{
				ExtractUrls(page);
			}
		}

		/// <summary>
		/// 如果找不到则不返回URL, 不然返回的URL太多
		/// </summary>
		/// <param name="page"></param>
		/// <param name="targetUrlExtractInfos"></param>
		protected virtual void ExtractUrls(Page page)
		{
			if (_targetUrlExtractors == null || _targetUrlExtractors.Count == 0)
			{
				return;
			}

			foreach (var targetUrlExtractor in _targetUrlExtractors)
			{
				var links = (page.Selectable.SelectList(targetUrlExtractor.Key)).Links().GetValues();
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

				foreach (var targetUrlPattern in targetUrlExtractor.Value)
				{
					foreach (string link in links)
					{
						if (Regex.IsMatch(targetUrlPattern, link))
						{
							page.AddTargetRequest(new Request(link, page.Request.Extras));
						}
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
						_targetUrlPatterns.Add(new Regex(pattern));
					}
				}
			}
			return _targetUrlPatterns;
		}

		protected virtual void AddTargetUrlExtractor(string regionXpath, params string[] patterns)
		{
			string xpath = string.IsNullOrEmpty(regionXpath?.Trim()) ? "." : regionXpath.Trim();

			var selector = Selectors.XPath(xpath);
			if (!_targetUrlExtractors.ContainsKey(selector))
			{
				_targetUrlExtractors.Add(selector, new List<string>());
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
				else
				{
					return;
				}
			}
			foreach (var pattern in patterns)
			{
				var realPattern = string.IsNullOrEmpty(pattern?.Trim()) ? null : pattern.Trim();
				if (!realPatterns.Any(p => p == realPattern))
				{
					realPatterns.Add(realPattern);
				}
			}
		}

		//protected virtual void AddTargetUrlRegion(string regionXpath)
		//{
		//	var xpath = string.IsNullOrEmpty(regionXpath) ? "." : regionXpath;
		//	var selector = Selectors.XPath(xpath);
		//	if (!_targetUrlExtractors.ContainsKey(selector))
		//	{
		//		_targetUrlExtractors.Add(selector, new List<Regex>());
		//	}
		//	var patterns = _targetUrlExtractors[selector];
		//	if (!patterns.Any(p => p.ToString() == regex))
		//	{
		//		patterns.Add(new Regex(regex));
		//	}
		//}

		/// <summary>
		/// Get the site settings
		/// </summary>
		public Site Site { get; set; }
	}
}
