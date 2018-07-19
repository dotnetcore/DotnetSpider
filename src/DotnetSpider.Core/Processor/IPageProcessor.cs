using DotnetSpider.Common;

namespace DotnetSpider.Core.Processor
{
	/// <summary>
	/// 页面解析器、抽取器
	/// </summary>
	public interface IPageProcessor
	{
		/// <summary>
		/// 解析数据结果, 解析目标链接
		/// </summary>
		/// <param name="page">页面数据</param>
		/// <param name="logger">日志接口</param>
		void Process(Page page, ILogger logger);
	}
}
