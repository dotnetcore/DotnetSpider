using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotnetSpider.Common;
using DotnetSpider.Extraction;

namespace DotnetSpider.Core.Processor.TargetRequestExtractors
{
	/// <summary>
	/// 指定区域下的链接并且需要符合给定正则的链接为符合要求的目标链接
	/// </summary>
	public sealed class RegionAndPatternTargetRequestExtractor : TargetRequestExtractor
	{
		private readonly Dictionary<ISelector, List<Regex>> _regionSelectorMapPatterns =
			new Dictionary<ISelector, List<Regex>>();

		/// <summary>
		/// 构造方法
		/// </summary>
		public RegionAndPatternTargetRequestExtractor()
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="regionXpath">目标链接所在区域</param>
		/// <param name="patterns">目标链接必须匹配的正则表达式</param>
		public RegionAndPatternTargetRequestExtractor(string regionXpath, params string[] patterns)
		{
			AddTargetUrlExtractor(regionXpath, patterns);
		}

		/// <summary>
		/// 解析出目标链接
		/// </summary>
		/// <param name="response">链接请求结果</param>
		/// <returns>目标链接</returns>
		protected override IEnumerable<Request> Extract(Response response)
		{
			if (_regionSelectorMapPatterns == null || _regionSelectorMapPatterns.Count == 0)
			{
				return new Request[0];
			}
			var site = response.Request.Site;

			List<string> resultUrls = new List<string>();
			foreach (var targetUrlExtractor in _regionSelectorMapPatterns)
			{
				if (Equals(targetUrlExtractor.Key, Selectors.Default()))
				{
					continue;
				}

				List<string> requests;

				if (response.ContentType == ContentType.Json)
				{
					requests = new List<string>(response.Selectable().SelectList(Selectors.Regex(RegexUtil.Url)).Links().GetValues());
				}
				else
				{
					requests = new List<string>(response.Selectable().SelectList(targetUrlExtractor.Key).Links().GetValues());
				}

				if (requests.Count == 0)
				{
					continue;
				}

				List<string> tmpRequests = new List<string>();
				foreach (string request in requests)
				{
#if !NETSTANDARD
					tmpRequests.Add(System.Web.HttpUtility.HtmlDecode(System.Web.HttpUtility.UrlDecode(request)));
#else
					tmpRequests.Add(System.Net.WebUtility.HtmlDecode(System.Net.WebUtility.UrlDecode(request)));
#endif
				}

				requests = tmpRequests;

				if (targetUrlExtractor.Value == null || targetUrlExtractor.Value.Count == 0)
				{
					resultUrls.AddRange(requests);
					continue;
				}

				foreach (var regex in targetUrlExtractor.Value)
				{
					foreach (string link in requests)
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
			var properties = new Dictionary<string, dynamic>();
			foreach (var kv in response.Request.Properties)
			{
				if (kv.Key != Env.UrlPropertyKey && kv.Key != Env.TargetUrlPropertyKey)
				{
					properties.Add(kv.Key, kv.Value);
				}
			}

			return resultUrls.Select(url => new Request(url, response.Request.Properties) { Site = site });
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
				throw new ArgumentException("Patterns should not be null or empty.");
			}

			var validPatterns = patterns.Where(p => p != null && !string.IsNullOrWhiteSpace(p.Trim())).Select(p => p.Trim())
				.ToList();

			if (validPatterns.Count != patterns.Length)
			{
				throw new ArgumentException("Pattern value should not be null or empty.");
			}

			ISelector selector = Selectors.Default();
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
			ISelector selector = Selectors.Default();
			if (!string.IsNullOrWhiteSpace(regionXpath))
			{
				selector = Selectors.XPath(regionXpath);
			}

			return _regionSelectorMapPatterns.ContainsKey(selector) ? _regionSelectorMapPatterns[selector] : null;
		}
	}
}
