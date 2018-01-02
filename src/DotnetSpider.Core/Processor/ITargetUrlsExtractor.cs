using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 目标链接的解析、筛选器
	/// </summary>
	public interface ITargetUrlsExtractor
	{
		/// <summary>
		/// 用于判断当前链接是否最后一个需要采集的链接, 如果是则不需要把解析到的目标链接添加到队列中
		/// </summary>
		ITargetUrlsExtractorTermination TargetUrlsExtractorTermination { get; set; }

		/// <summary>
		/// 目标链接必须符合的正则表达式
		/// </summary>
		List<Regex> TargetUrlPatterns { get; }

		/// <summary>
		/// 如果目标链接符合正则表达式，则需要排除不添加到目标链接队列中
		/// </summary>
		List<Regex> ExcludeTargetUrlPatterns { get; }

		/// <summary>
		/// 添加目标链接必须符合的正则表达式
		/// </summary>
		/// <param name="patterns">正则表达式</param>
		void AddTargetUrlPatterns(params string[] patterns);

		/// <summary>
		/// 添加排除目标链接的正则表达式
		/// </summary>
		/// <param name="patterns">正则表达式</param>
		void AddExcludeTargetUrlPatterns(params string[] patterns);

		/// <summary>
		/// 解析出目标链接, 返回Request的设计是因为有可能需要重新计算PostBody等值, 因此不能直接返回string
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <param name="site">站点信息</param>
		/// <returns>目标链接</returns>
		IEnumerable<Request> ExtractRequests(Page page, Site site);
	}
}
