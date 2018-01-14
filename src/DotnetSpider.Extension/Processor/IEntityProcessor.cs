using DotnetSpider.Extension.Model;

namespace DotnetSpider.Extension.Processor
{
	/// <summary>
	/// 针对爬虫实体类的页面解析器、抽取器
	/// </summary>
	public interface IEntityProcessor
	{
		/// <summary>
		/// 爬虫实体类的定义
		/// </summary>
		IEntityDefine EntityDefine { get; }
	}
}
