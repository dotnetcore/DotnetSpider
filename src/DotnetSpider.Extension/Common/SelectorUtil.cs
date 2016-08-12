using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace DotnetSpider.Extension.Common
{
	public class SelectorUtil
	{
		public static ISelector Parse(Selector selector)
		{
			if (string.IsNullOrEmpty(selector?.Expression))
			{
				return null;
			}

			string expression = selector.Expression;

			switch (selector.Type)
			{
				case ExtractType.Css:
					{
						return Selectors.Css(expression);
					}
				case ExtractType.Enviroment:
					{
						return Selectors.Enviroment (expression);
					}
				case ExtractType.JsonPath:
					{
						return Selectors.JsonPath(expression);
					}
				case ExtractType.Regex:
					{
						if (string.IsNullOrEmpty(selector.Argument?.ToString()))
						{
							return Selectors.Regex(expression);
						}
						else
						{
							int group;
							if (int.TryParse(selector.Argument.ToString(), out group))
							{
								return Selectors.Regex(expression, group);
							}
							throw new SpiderException("Regex argument shoulb be a number set to group: " + selector);
						}
					}
				case ExtractType.XPath:
					{
						return Selectors.XPath(expression);
					}
			}
			throw new SpiderException("Not support selector: " + selector);
		}

        /// <summary>
        /// 如果找不到则不返回URL, 不然返回的URL太多
        /// </summary>
        /// <param name="item"></param>
        /// <param name="page"></param>
        /// <param name="targetUrlExtractInfos"></param>
        internal static void ExtractLinks(ISelectable item, Page page, List<Processor.EntityProcessor.TargetUrlExtractorInfo> targetUrlExtractInfos)
        {
            if (targetUrlExtractInfos == null)
            {
                return;
            }

            foreach (var targetUrlExtractInfo in targetUrlExtractInfos)
            {
                var urlRegionSelector = targetUrlExtractInfo.Region;
                var formatters = targetUrlExtractInfo.Formatters;
                var urlPatterns = targetUrlExtractInfo.Patterns;
                var extras = targetUrlExtractInfo.Extras;
                var selectable = item ?? page.Selectable;
                var links = urlRegionSelector == null ? selectable.Links().GetValues() : (selectable.SelectList(urlRegionSelector)).Links().GetValues();
                if (links == null)
                {
                    return;
                }

                // check: 仔细考虑是放在前面, 还是在后面做 formatter, 我倾向于在前面. 对targetUrl做formatter则表示Start Url也应该是要符合这个规则的。
                if (formatters != null && formatters.Count > 0)
                {
                    List<string> tmp = new List<string>();
                    foreach (string link in links)
                    {
                        var url = new String(link.ToCharArray());
                        foreach (Formatter f in formatters)
                        {
                            url = f.Formate(url);
                        }
                        tmp.Add(url);
                    }
                    links = tmp;
                }

                List<string> tmpLinks = new List<string>();
                foreach (var link in links)
                {
#if !NET_CORE
                    tmpLinks.Add(HttpUtility.HtmlDecode(HttpUtility.UrlDecode(link)));
#else
					tmpLinks.Add(WebUtility.HtmlDecode(WebUtility.UrlDecode(link)));
#endif
                }
                links = tmpLinks;
                var allExtras = new Dictionary<string, dynamic>();
                foreach (var extra in page.Request.Extras.Union(extras))
                {
                    allExtras.Add(extra.Key, extra.Value);
                }

                if (urlPatterns == null || urlPatterns.Count == 0)
                {
                    //page.AddTargetRequests(links);
                    foreach (var link in links)
                    {
                        page.AddTargetRequest(new Request(link, page.Request.NextDepth, allExtras));
                    }
                    return;
                }

                foreach (Regex targetUrlPattern in urlPatterns)
                {
                    foreach (string link in links)
                    {
                        if (targetUrlPattern.IsMatch(link))
                        {
                            page.AddTargetRequest(new Request(link, page.Request.NextDepth, allExtras));
                        }
                    }
                }
            }
        }

        public static void ExtractLinks(ISelectable item, Page page, List<TargetUrlExtractor> targetUrlExtractors)
        {
            ExtractLinks(item, page, _transform(targetUrlExtractors));
        }
        private static List<Processor.EntityProcessor.TargetUrlExtractorInfo> _transform(List<TargetUrlExtractor> targetUrlExtractors)
        {
            List<Processor.EntityProcessor.TargetUrlExtractorInfo> result = new List<Processor.EntityProcessor.TargetUrlExtractorInfo>();
            if (targetUrlExtractors != null)
            {
                foreach (var targetUrlExtractor in targetUrlExtractors)
                {
                    result.Add(new Processor.EntityProcessor.TargetUrlExtractorInfo
                    {
                        Patterns = targetUrlExtractor.Patterns.Select(t => new Regex(t)).ToList(),
                        Formatters = targetUrlExtractor.Formatters,
                        Region = SelectorUtil.Parse(targetUrlExtractor.Region),
                        Extras = targetUrlExtractor.Extras
                    });
                }
            }
            return result;
        }
    }
}
