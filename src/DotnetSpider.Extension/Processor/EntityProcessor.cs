using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extraction.Model;

namespace DotnetSpider.Extension.Processor
{
	/// <summary>
	/// 针对爬虫实体类的页面解析器、抽取器
	/// </summary>
	public class EntityProcessor<T> : ModelProcessor where T : new()
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="extractor">爬虫实体的解析器</param>
		/// <param name="targetRequestExtractor">目标链接的解析、筛选器</param>
		/// <param name="dataHandlers">对解析的结果进一步加工操作</param>
		public EntityProcessor(IModelExtractor extractor = null, ITargetRequestExtractor targetRequestExtractor = null, params IDataHandler[] dataHandlers)
			: base(new ModelDefinition<T>(), extractor ?? new ModelExtractor<T>(), targetRequestExtractor, dataHandlers)
		{
		}
	}
}
