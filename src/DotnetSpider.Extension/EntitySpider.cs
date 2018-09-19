using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extraction.Model;

namespace DotnetSpider.Extension
{
	/// <summary>
	/// 实体类爬虫的定义
	/// </summary>
	public abstract class EntitySpider : DistributedSpider
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public EntitySpider() : this(null)
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="name">名称</param>
		/// <param name="site">目标站点信息</param>
		public EntitySpider(string name)
		{
			if (!string.IsNullOrWhiteSpace(name))
			{
				Name = name;
			}
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		public void AddEntityType<T>() where T : IBaseEntity
		{
			AddEntityType<T>(null, null);
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		public void AddEntityType<T>(IDataHandler dataHandler) where T : IBaseEntity
		{
			AddEntityType<T>(null, dataHandler);
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		public void AddEntityType<T>(ITargetRequestExtractor targetUrlsExtractor) where T : IBaseEntity
		{
			AddEntityType<T>(targetUrlsExtractor, null);
		}

		/// <summary>
		/// 添加爬虫实体类
		/// </summary>
		/// <typeparam name="T">爬虫实体类的类型, 必须继承自 ISpiderEntity</typeparam>
		/// <param name="targetUrlsExtractor">目标链接的解析、筛选器</param>
		/// <param name="dataHandler">对解析的结果进一步加工操作</param>
		public void AddEntityType<T>(ITargetRequestExtractor targetUrlsExtractor, IDataHandler dataHandler) where T : IBaseEntity
		{
			CheckIfRunning();

			var processor = new EntityProcessor<T>(new ModelExtractor<T>(), targetUrlsExtractor, dataHandler);
			AddPageProcessors(processor);
		}

		/// <summary>
		/// Get the default pipeline when user forget set a pepeline to spider.
		/// </summary>
		/// <returns>数据管道</returns>
		protected override IPipeline GetDefaultPipeline()
		{
			return DbEntityPipeline.GetPipelineFromAppConfig();
		}
	}
}