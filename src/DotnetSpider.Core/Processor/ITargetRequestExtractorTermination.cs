using DotnetSpider.Common;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 目标链接解析器的中止器
	/// </summary>
	public interface ITargetRequestExtractorTermination
	{
		/// <summary>
		/// Return true, skip all urls from target urls extractor.
		/// </summary>
		/// <param name="response">链接请求结果</param>
		/// <returns>是否到了最终一个链接</returns>
		bool IsTerminated(Response response);
	}
}
