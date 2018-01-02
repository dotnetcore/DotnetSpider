using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Processor
{
	public abstract class TargetUrlsExtractor : ITargetUrlsExtractor
	{
		public List<Regex> TargetUrlPatterns { get; protected set; }

		public List<Regex> ExcludeTargetUrlPatterns { get; protected set; }

		public ITargetUrlsExtractorTermination TargetUrlsExtractorTermination { get; set; }

		public void AddTargetUrlPatterns(params string[] patterns)
		{
			if (patterns != null)
			{
				if (TargetUrlPatterns == null)
				{
					TargetUrlPatterns = new List<Regex>();
				}
				foreach (var pattern in patterns)
				{
					if (TargetUrlPatterns.All(p => p.ToString() != pattern))
					{
						TargetUrlPatterns.Add(new Regex(pattern));
					}
				}
			}
		}

		public void AddExcludeTargetUrlPatterns(params string[] patterns)
		{
			if (patterns != null)
			{
				if (ExcludeTargetUrlPatterns == null)
				{
					ExcludeTargetUrlPatterns = new List<Regex>();
				}
				foreach (var pattern in patterns)
				{
					if (ExcludeTargetUrlPatterns.All(p => p.ToString() != pattern))
					{
						ExcludeTargetUrlPatterns.Add(new Regex(pattern));
					}
				}
			}
		}

		public IEnumerable<Request> ExtractRequests(Page page, Site site)
		{
			if (TargetUrlsExtractorTermination != null && TargetUrlsExtractorTermination.IsTermination(page))
			{
				return new Request[0];
			}
			return Extract(page, site);
		}

		protected abstract IEnumerable<Request> Extract(Page page, Site site);
	}
}