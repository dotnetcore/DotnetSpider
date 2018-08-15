using DotnetSpider.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 目标链接的解析、筛选器的抽象
	/// </summary>
	public abstract class TargetRequestExtractor : ITargetRequestExtractor
	{
		/// <summary>
		/// 目标链接必须符合的正则表达式
		/// </summary>
		public HashSet<Regex> TargetUrlPatterns { get; protected set; } = new HashSet<Regex>();

		/// <summary>
		/// 如果目标链接符合正则表达式，则需要排除不添加到目标链接队列中
		/// </summary>
		public HashSet<Regex> ExcludeTargetUrlPatterns { get; protected set; } = new HashSet<Regex>();

		/// <summary>
		/// 用于判断当前链接是否最后一个需要采集的链接, 如果是则不需要把解析到的目标链接添加到队列中
		/// </summary>
		public ITargetRequestExtractorTermination Termination { get; set; }

		/// <summary>
		/// 添加目标链接必须符合的正则表达式
		/// </summary>
		/// <param name="patterns">正则表达式</param>
		public void AddTargetUrlPatterns(params string[] patterns)
		{
			if (patterns != null)
			{
				if (TargetUrlPatterns == null)
				{
					TargetUrlPatterns = new HashSet<Regex>();
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
		/// 添加排除目标链接的正则表达式
		/// </summary>
		/// <param name="patterns">正则表达式</param>
		public void AddExcludeTargetUrlPatterns(params string[] patterns)
		{
			if (patterns != null)
			{
				if (ExcludeTargetUrlPatterns == null)
				{
					ExcludeTargetUrlPatterns = new HashSet<Regex>();
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
		/// 解析出目标链接, 返回Request的设计是因为有可能需要重新计算PostBody等值, 因此不能直接返回string
		/// </summary>
		/// <param name="response">链接请求结果</param>
		/// <returns>目标链接</returns>
		public IEnumerable<Request> ExtractRequests(Response response)
		{
			if (Termination != null && Termination.IsTerminated(response))
			{
				return new Request[0];
			}
			return Extract(response);
		}

		/// <summary>
		/// 具体的抽取实现
		/// </summary>
		/// <param name="response">链接请求结果</param>
		/// <returns>目标链接</returns>
		protected abstract IEnumerable<Request> Extract(Response response);
	}
}
