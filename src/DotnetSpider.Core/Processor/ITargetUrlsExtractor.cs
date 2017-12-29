using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 目标链接的解析、筛选器
	/// </summary>
	public interface ITargetUrlsExtractor
	{
		ITargetUrlsExtractorTermination TerminationDetector { get; set; }
		List<Regex> TargetUrlPatterns { get; }
		List<Regex> ExcludeTargetUrlPatterns { get; }
		void AddTargetUrlPatterns(params string[] patterns);
		void AddExcludeTargetUrlPatterns(params string[] patterns);
		IEnumerable<string> ExtractUrls(Page page, Site site);
	}
}
