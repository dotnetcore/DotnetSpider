using DotnetSpider.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotnetSpider.Core.Downloader
{
	public abstract class BaseTargetUrlsBuilder : IAfterDownloadCompleteHandler
	{
		/// <summary>
		/// http://a.com?p=40  PaggerString: p=40 Pattern: p=\d+
		/// </summary>
		public string PaggerString { get; protected set; }

		public Regex PaggerPattern { get; protected set; }

		public virtual void Handle(ref Page page, ISpider spider)
		{
			if (!string.IsNullOrEmpty(page?.Content) && !string.IsNullOrEmpty(PaggerString) && (Termination == null || !Termination.IsTermination(page, this)))
			{
				page.AddTargetRequests(GenerateRequests(page));
				page.SkipExtractTargetUrls = true;
			}
		}

		public ITargetUrlsBuilderTermination Termination { get; set; }

		protected abstract IList<Request> GenerateRequests(Page page);

		protected BaseTargetUrlsBuilder(string paggerString)
		{
			PaggerString = paggerString;
			PaggerPattern = new Regex($"{RegexUtil.NumRegex.Replace(PaggerString, @"\d+")}");
		}

		public virtual string GetCurrentPagger(string currentUrlOrContent)
		{
			return PaggerPattern.Match(currentUrlOrContent).Value;
		}
	}
}
