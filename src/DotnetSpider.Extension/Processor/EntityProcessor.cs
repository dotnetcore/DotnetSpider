using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;

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
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		/// <param name="tableName">实体在数据库中的表名, 此优先级高于EntitySelector中的定义</param>
		public EntityProcessor(ITargetUrlsExtractor targetUrlsExtractor, params IDataHandler[] dataHandlers) : this(null, targetUrlsExtractor, dataHandlers)
		{
		}
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="extractor">爬虫实体的解析器</param>
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		/// <param name="tableName">实体在数据库中的表名, 此优先级高于EntitySelector中的定义</param>
		public EntityProcessor(IModelExtractor extractor = null, ITargetUrlsExtractor targetUrlsExtractor = null, params IDataHandler[] dataHandlers) : base(new ModelDefine<T>(), extractor, targetUrlsExtractor, dataHandlers)
		{
		}
	}
}
