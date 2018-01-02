using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 目标链接的解析、筛选器
	/// </summary>
	public interface ITargetUrlsExtractor
	{
		ITargetUrlsExtractorTermination TargetUrlsExtractorTermination { get; set; }
		List<Regex> TargetUrlPatterns { get; }
		List<Regex> ExcludeTargetUrlPatterns { get; }
		void AddTargetUrlPatterns(params string[] patterns);
		void AddExcludeTargetUrlPatterns(params string[] patterns);
		/// <summary>
		/// 解析出目标链接, 返回Request的设计是因为有可能需要重新计算PostBody等值, 因此不能直接返回string
		/// </summary>
		/// <param name="page"></param>
		/// <param name="site"></param>
		/// <returns></returns>
		IEnumerable<Request> ExtractRequests(Page page, Site site);
	}
}
