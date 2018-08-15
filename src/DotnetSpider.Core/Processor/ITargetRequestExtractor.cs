using DotnetSpider.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 目标链接的解析、筛选器
	/// </summary>
	public interface ITargetRequestExtractor
	{
		/// <summary>
		/// 用于判断当前链接是否最后一个需要采集的链接, 如果是则不需要把解析到的目标链接添加到队列中
		/// </summary>
		ITargetRequestExtractorTermination Termination { get; set; }

		/// <summary>
		/// 目标链接必须符合的正则表达式
		/// </summary>
		HashSet<Regex> TargetUrlPatterns { get; }

		/// <summary>
		/// 如果目标链接符合正则表达式，则需要排除不添加到目标链接队列中
		/// </summary>
		HashSet<Regex> ExcludeTargetUrlPatterns { get; }

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
		/// <param name="response">链接请求结果</param>
		/// <returns>目标链接</returns>
		IEnumerable<Request> ExtractRequests(Response response);
	}
}
