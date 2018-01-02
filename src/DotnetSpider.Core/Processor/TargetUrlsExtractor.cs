using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 目标链接的解析、筛选器的抽象
	/// </summary>
	public abstract class TargetUrlsExtractor : ITargetUrlsExtractor
	{
		/// <summary>
		/// <see cref="ITargetUrlsExtractor.TargetUrlPatterns"/>
		/// </summary>
		public List<Regex> TargetUrlPatterns { get; protected set; }

		/// <summary>
		/// <see cref="ITargetUrlsExtractor.ExcludeTargetUrlPatterns"/>
		/// </summary>
		public List<Regex> ExcludeTargetUrlPatterns { get; protected set; }

		/// <summary>
		/// <see cref="ITargetUrlsExtractor.TargetUrlsExtractorTermination"/>
		/// </summary>
		public ITargetUrlsExtractorTermination TargetUrlsExtractorTermination { get; set; }

		/// <summary>
		/// <see cref="ITargetUrlsExtractor.AddTargetUrlPatterns(string[])"/>
		/// </summary>
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

		/// <summary>
		/// <see cref="ITargetUrlsExtractor.AddExcludeTargetUrlPatterns(string[])"/>
		/// </summary>
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

		/// <summary>
		/// <see cref="ITargetUrlsExtractor.ExtractRequests(Page, Site)"/>
		/// </summary>
		public IEnumerable<Request> ExtractRequests(Page page, Site site)
		{
			if (TargetUrlsExtractorTermination != null && TargetUrlsExtractorTermination.IsTermination(page))
			{
				return new Request[0];
			}
			return Extract(page, site);
		}

		/// <summary>
		/// 具体的抽取实现
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <param name="site">站点信息</param>
		/// <returns>目标链接</returns>
		protected abstract IEnumerable<Request> Extract(Page page, Site site);
	}
}