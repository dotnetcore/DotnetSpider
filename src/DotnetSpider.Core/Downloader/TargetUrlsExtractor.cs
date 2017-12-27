using DotnetSpider.Core.Infrastructure;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Downloader
{
	/// <summary>
	/// 目标链接的解析、筛选器
	/// </summary>
	public interface ITargetUrlsExtractor : IAfterDownloadCompleteHandler
	{
		ITargetUrlsExtractorTermination TerminationDetector { get; }
		IEnumerable<Request> ExtractRequests(Page page);
	}

	public abstract class TargetUrlsExtractor : ITargetUrlsExtractor
	{
		private readonly Regex _pagerPattern;

		public ITargetUrlsExtractorTermination TerminationDetector { get; private set; }

		/// <summary>
		/// http://a.com?p=40  PaggerString: p=40 Pattern: p=\d+
		/// </summary>
		public readonly string PagerString;

		public abstract IEnumerable<Request> ExtractRequests(Page page);

		protected TargetUrlsExtractor(string pagerString, ITargetUrlsExtractorTermination termination)
		{
			if (string.IsNullOrEmpty(pagerString) || string.IsNullOrWhiteSpace(pagerString))
			{
				throw new SpiderException("pagerString should not be null.");
			}

			PagerString = pagerString;
			_pagerPattern = new Regex($"{RegexUtil.NumRegex.Replace(PagerString, @"\d+")}");
			TerminationDetector = termination;
		}

		public virtual void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content) && !string.IsNullOrEmpty(PagerString) &&
				(TerminationDetector == null || !TerminationDetector.IsTermination(page, this)))
			{
				page.AddTargetRequests(ExtractRequests(page));
				page.SkipExtractTargetUrls = true;
			}
		}

		public string GetCurrentPagger(string currentUrlOrContent)
		{
			return _pagerPattern.Match(currentUrlOrContent).Value;
		}
	}
}