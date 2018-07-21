using DotnetSpider.Extraction.Model;

namespace DotnetSpider.Extension.Processor
{
	/// <summary>
	/// 针对爬虫模型的页面解析器、抽取器
	/// </summary>
	public interface IModelProcessor
	{
		/// <summary>
		/// 爬虫模型的定义
		/// </summary>
		IModel Model { get; }
	}
}
