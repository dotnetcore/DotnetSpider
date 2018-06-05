using DotnetSpider.Core.Infrastructure;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Downloader
{
	public abstract class BaseTargetUrlsBuilder : IAfterDownloadCompleteHandler
	{
		/// <summary>
		/// http://a.com?p=40  PaggerString: p=40 Pattern: p=\d+
		/// </summary>
		protected readonly string PagerString;
		private readonly Regex _pagerPattern;
		private readonly ITargetUrlsBuilderTermination _termination;

		protected abstract IList<Request> GenerateRequests(Page page);

		protected BaseTargetUrlsBuilder(string pagerString, ITargetUrlsBuilderTermination termination)
		{
			if (string.IsNullOrEmpty(pagerString))
			{
				throw new SpiderException("pagerString should not be null.");
			}

			PagerString = pagerString;
			_pagerPattern = new Regex($"{RegexUtil.NumRegex.Replace(PagerString, @"\d+")}");
			_termination = termination;
		}

		public virtual void Handle(ref Page page,IDownloader downloader, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content) && !string.IsNullOrEmpty(PagerString) &&
				(_termination == null || !_termination.IsTermination(page, this)))
			{
				page.AddTargetRequests(GenerateRequests(page));
				page.SkipExtractTargetUrls = true;
			}
		}

		public string GetCurrentPagger(string currentUrlOrContent)
		{
			return _pagerPattern.Match(currentUrlOrContent).Value;
		}
	}
}